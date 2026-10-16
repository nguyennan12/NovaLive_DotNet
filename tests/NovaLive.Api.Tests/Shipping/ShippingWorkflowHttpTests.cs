using NovaLive.Application.Abstractions.Auth;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NovaLive.Api.Tests.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Products;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Api.Tests.Shipping;

[Collection("API host lifecycle")]
public sealed class ShippingWorkflowHttpTests
{
    private const string CreatePath = "/api/v1/seller/shipping/create-order";

    private static async Task<(CreateShippingOrderRequest Request, Guid ParentId)> Seed(
        WebApplicationFactory<Program> app, HttpClient client, DateTimeOffset now)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Mock.Get(scope.ServiceProvider.GetRequiredService<ICacheService>())
            .Setup(c => c.GetAsync<string[]>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string[]?)null);
        var user = new User($"{Guid.NewGuid()}@example.com", "hash", "Seller", null); user.Activate();
        var shop = new Shop(user.Id, "Shipping shop", $"shipping-{Guid.NewGuid():N}"); shop.Approve();
        var warehouse = new ShopAddress(shop.Id, "Warehouse", "Seller", "0900000000", 1, "Province",
            100, "District", "W100", "Ward", "Pickup street");
        var parent = ParentOrder.Create(user.Id, $"NOVA-{Guid.NewGuid():N}", JsonSerializer.Serialize(new
        {
            recipientName = "Snapshot buyer", phone = "0911111111", provinceId = 1, provinceName = "Province",
            districtId = 200, districtName = "District", wardCode = "W200", wardName = "Ward", detailAddress = "Snapshot street"
        }), 200, 30, 0, 230, null, now);
        var spu = new Spu(shop.Id, Guid.NewGuid(), "Product", null, null, null, null);
        var sku = new Sku(spu.Id, shop.Id, "SKU1", null, 100, 100, 350);
        var line = new OrderItemCalculationResult(sku.Id, spu.Id, 2, 100, 200, 0, 0, 0, 200);
        var sub = SubOrder.Create(parent.Id, $"SUB-{Guid.NewGuid():N}", warehouse.Id,
            new ShopCalculationResult(shop.Id, 200, 30, 0, 0, 0, 200, 230, [line]), true, now);
        db.Users.Add(user); db.UserRoles.Add(new UserRole(user.Id, SystemRoleIds.Seller, now));
        db.Shops.Add(shop); db.ShopAddresses.Add(warehouse); db.Spus.Add(spu); db.Skus.Add(sku);
        db.ParentOrders.Add(parent); db.SubOrders.Add(sub);
        db.Payments.Add(Payment.Create(parent.Id, PaymentMethod.COD, 230, now));
        db.OrderItems.Add(OrderItem.Create(sub.Id, """{"skuCode":"SKU1","spuName":"Product"}""", 100, line));
        await db.SaveChangesAsync();
        var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(user.Id, user.Email, ["Seller"], shop.Id);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return (new(sub.Id, "GHN", "2", warehouse.Id), parent.Id);
    }

    [Fact]
    public async Task RealPipeline_RollsBackCarrierFailure_ThenCreatesOnceAndServesLabelAndTracking()
    {
        var provider = new Mock<IShippingProvider>(MockBehavior.Strict);
        provider.SetupGet(p => p.ProviderCode).Returns("GHN");
        var fail = true;
        provider.Setup(p => p.CreateShipmentAsync(It.IsAny<ShippingShipmentCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShippingShipmentCreateModel model, CancellationToken _) =>
            {
                model.WeightGram.Should().Be(700); model.CodAmount.Should().Be(230);
                model.ToAddress.Should().Be("Snapshot street");
                return fail ? ShippingErrors.ProviderError("GHN.Unavailable", "offline")
                    : Result<ShippingOrderCreateResult>.Success(new("GHN-1", "GHN-1", 32000));
            });
        provider.Setup(p => p.GetShippingLabelAsync("GHN-1", It.IsAny<CancellationToken>())).ReturnsAsync("%PDF-1.7"u8.ToArray());
        provider.Setup(p => p.GetTrackingAsync("GHN-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingTrackingResult("delivering", []));
        using var factory = new AuthApplicationFactory();
        using var app = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IShippingProvider>(); services.AddSingleton(provider.Object);
        }));
        using var client = app.CreateClient(); var seed = await Seed(app, client, factory.Clock.UtcNow);
        var request = seed.Request;
        (await client.PostAsJsonAsync(CreatePath, request)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.SubOrders.SingleAsync()).Status.Should().Be(SubOrderStatus.Confirmed);
            (await db.ShippingOrders.CountAsync()).Should().Be(0);
            (await db.OrderStatusHistories.CountAsync()).Should().Be(0);
            (await db.OutboxMessages.CountAsync()).Should().Be(0);
        }
        fail = false;
        var created = await client.PostAsJsonAsync(CreatePath, request);
        created.StatusCode.Should().Be(HttpStatusCode.Created, await created.Content.ReadAsStringAsync());
        (await client.PostAsJsonAsync(CreatePath, request)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.SubOrders.SingleAsync()).Status.Should().Be(SubOrderStatus.Shipping);
            (await db.ShippingOrders.SingleAsync()).WeightGram.Should().Be(700);
            (await db.OrderStatusHistories.SingleAsync()).ChangedByRole.Should().Be(OrderRole.Seller);
            (await db.OutboxMessages.SingleAsync()).EventType.Should().Be(nameof(ShipmentCreatedEvent));
        }
        var label = await client.GetAsync($"/api/v1/seller/shipping/{request.SubOrderId}/label");
        label.StatusCode.Should().Be(HttpStatusCode.OK);
        label.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        (await label.Content.ReadAsByteArrayAsync()).Should().Equal("%PDF-1.7"u8.ToArray());
        var live = await client.GetFromJsonAsync<ApiResponse<List<TrackingResponse>>>(
            $"/api/v1/orders/{seed.ParentId}/tracking");
        live!.Data.Should().ContainSingle().Which.IsStale.Should().BeFalse();
        provider.Setup(p => p.GetTrackingAsync("GHN-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ShippingErrors.ProviderError("GHN.Unavailable", "offline"));
        var fallback = await client.GetFromJsonAsync<ApiResponse<List<TrackingResponse>>>(
            $"/api/v1/seller/shipping/{request.SubOrderId}/tracking");
        fallback!.Data.Should().ContainSingle().Which.IsStale.Should().BeTrue();
        provider.Verify(p => p.CreateShipmentAsync(It.IsAny<ShippingShipmentCreateModel>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task PersistenceFailure_RollsBackShipmentSubOrderAndHistory()
    {
        var provider = new Mock<IShippingProvider>();
        provider.SetupGet(p => p.ProviderCode).Returns("GHN");
        provider.Setup(p => p.CreateShipmentAsync(It.IsAny<ShippingShipmentCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingOrderCreateResult("GHN-1", "GHN-1", 32000));
        var outbox = new Mock<IOutboxRepository>();
        outbox.Setup(o => o.AddAsync(It.IsAny<NovaLive.Domain.System.OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Injected persistence failure"));
        using var factory = new AuthApplicationFactory();
        using var app = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IShippingProvider>(); services.AddSingleton(provider.Object);
            services.RemoveAll<IOutboxRepository>(); services.AddSingleton(outbox.Object);
        }));
        using var client = app.CreateClient(); var seed = await Seed(app, client, factory.Clock.UtcNow);
        (await client.PostAsJsonAsync(CreatePath, seed.Request)).StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sub = await db.SubOrders.SingleAsync();
        sub.Status.Should().Be(SubOrderStatus.Confirmed); sub.ShippedAt.Should().BeNull();
        (await db.ShippingOrders.CountAsync()).Should().Be(0);
        (await db.OrderStatusHistories.CountAsync()).Should().Be(0);
        (await db.OutboxMessages.CountAsync()).Should().Be(0);
    }
}
