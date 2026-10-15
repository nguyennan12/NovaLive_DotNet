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
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Carts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Products;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Api.Tests.Orders;

[Collection("API host lifecycle")]
public sealed class CheckoutHttpTests
{
    private const string Endpoint = "/api/v1/orders/calculate-checkout";

    private static async Task<Guid> Authenticate(WebApplicationFactory<Program> app, HttpClient client, bool withPermission = true)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Mock.Get(scope.ServiceProvider.GetRequiredService<ICacheService>())
            .Setup(x => x.GetAsync<string[]>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((string[]?)null);
        var user = new User($"{Guid.NewGuid()}@example.com", "hash", "Buyer", null); user.Activate();
        db.Users.Add(user);
        db.UserRoles.Add(new UserRole(user.Id, withPermission ? SystemRoleIds.Buyer : Guid.NewGuid(), DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(user.Id, user.Email, ["Buyer"], null);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return user.Id;
    }

    [Fact]
    public async Task RealRoute_EnforcesAuthenticationPermissionAndValidation()
    {
        using var app = new AuthApplicationFactory(); using var client = app.CreateClient();
        var request = new CalculateCheckoutRequest([Guid.NewGuid()], Guid.NewGuid(), null, null, null);
        (await client.PostAsJsonAsync(Endpoint, request)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await Authenticate(app, client, withPermission: false);
        (await client.PostAsJsonAsync(Endpoint, request)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        await Authenticate(app, client);
        (await client.PostAsJsonAsync(Endpoint, request with { CartItemIds = [] })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PostAsJsonAsync(Endpoint, request with { CartItemIds = [Guid.Empty] })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PostAsJsonAsync(Endpoint, request)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RealPreview_UsesRepositoriesAndDomainAcrossTwoShops_AndLeavesAllDatabaseStateUnchanged()
    {
        var provider = new Mock<IShippingProvider>(MockBehavior.Strict);
        provider.Setup(x => x.CalculateFeeAsync(It.IsAny<ShippingFeeCalculationModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShippingFeeCalculationModel model, CancellationToken _) =>
                Result.Success(new ShippingFeeQuote("GHN", "2", model.FromDistrictId == 100 ? 30m : 40m)));
        using var factory = new AuthApplicationFactory();
        using var app = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IShippingProvider>(); services.AddSingleton(provider.Object);
        }));
        using var client = app.CreateClient(); var userId = await Authenticate(app, client);
        CalculateCheckoutRequest request;
        Guid foreignAddressId;
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = factory.Clock.UtcNow;
            var address = new UserAddress(userId, "Buyer", "0", 1, "P", 200, "D", "W200", "W", "Street");
            var foreignAddress = new UserAddress(Guid.NewGuid(), "Other", "0", 1, "P", 201, "D", "W201", "W", "Street");
            foreignAddressId = foreignAddress.Id;
            db.UserAddresses.AddRange(address, foreignAddress);
            var cart = new Cart(userId, now.AddDays(-1)); db.Carts.Add(cart);
            var selected = new List<Guid>(); Guid voucherShopId = default;
            for (var i = 0; i < 2; i++)
            {
                var shop = new Shop(Guid.NewGuid(), $"Shop {i}", $"checkout-{i}"); shop.Approve();
                var spu = new Spu(shop.Id, Guid.NewGuid(), $"Product {i}", null, null, null, null);
                var sku = new Sku(spu.Id, shop.Id, $"SKU-{i}", null, 100m, 100m);
                var stock = new NovaLive.Domain.Inventory.Inventory(sku.Id, shop.Id, 10); stock.Reserve(3);
                var primary = new ShopAddress(shop.Id, "Primary", "Seller", "0", 1, "P", 100 + i, "D", $"W{i}", "W", "Street"); primary.SetPrimary(true);
                var secondary = new ShopAddress(shop.Id, "Secondary", "Seller", "0", 1, "P", 999, "D", "WRONG", "W", "Street");
                var item = new CartItem(Guid.NewGuid(), cart.Id, sku.Id, shop.Id, i == 0 ? 2 : 1, 1m, now.AddDays(-1));
                db.Shops.Add(shop); db.Spus.Add(spu); db.Skus.Add(sku); db.Inventories.Add(stock);
                db.ShopAddresses.AddRange(secondary, primary); db.CartItems.Add(item); selected.Add(item.Id);
                if (i == 0) voucherShopId = shop.Id;
            }
            db.CartItems.Add(new CartItem(Guid.NewGuid(), cart.Id, Guid.NewGuid(), Guid.NewGuid(), 100, 999m));
            Discount Voucher(string code, Guid? shopId, DiscountType type, decimal value) =>
                new(shopId, code, code, type, value, 0m, null, 100, 1, DiscountAppliesTo.AllProducts,
                    null, now.AddDays(-1), now.AddDays(1), true);
            db.Discounts.AddRange(Voucher("SHOP", voucherShopId, DiscountType.FixedCart, 30m),
                Voucher("PLATFORM", null, DiscountType.FixedCart, 101m), Voucher("SHIP", null, DiscountType.FreeShip, 17m));
            await db.SaveChangesAsync();
            request = new(selected, address.Id, [new(voucherShopId, "SHOP")], "PLATFORM", "SHIP");
        }

        var before = await CaptureState(app);
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var response = await client.PostAsJsonAsync(Endpoint, request);
            response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
            var preview = (await response.Content.ReadFromJsonAsync<ApiResponse<CheckoutPreviewResponse>>())!.Data!;
            preview.TotalItemsAmount.Should().Be(300m);
            preview.TotalShippingFee.Should().Be(70m);
            preview.TotalShopDiscount.Should().Be(30m);
            preview.TotalPlatformDiscount.Should().Be(101m);
            preview.TotalShippingDiscount.Should().Be(17m);
            preview.GrandTotal.Should().Be(222m).And.Be(preview.ShopPreviews.Sum(s => s.ShopTotal));
            preview.ShopPreviews.Should().HaveCount(2);
            preview.ShopPreviews.SelectMany(s => s.Items).Should().OnlyContain(i => i.Price == 100m);
        }
        (await client.PostAsJsonAsync(Endpoint, request with { ShippingAddressId = foreignAddressId }))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.PostAsJsonAsync(Endpoint, request with { CartItemIds = [request.CartItemIds[0], Guid.NewGuid()] }))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.PostAsJsonAsync(Endpoint, request with { PlatformProductVoucherCode = "MISSING" }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await CaptureState(app)).Should().Be(before);
        provider.Verify(x => x.CalculateFeeAsync(It.Is<ShippingFeeCalculationModel>(m =>
            m.FromDistrictId == 100 && m.FromWardCode == "W0" && m.WeightGram == 400 && m.ToDistrictId == 200),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
        provider.Verify(x => x.CalculateFeeAsync(It.Is<ShippingFeeCalculationModel>(m =>
            m.FromDistrictId == 101 && m.FromWardCode == "W1" && m.WeightGram == 200 && m.ToWardCode == "W200"),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
        provider.VerifyNoOtherCalls();
    }

    private static async Task<string> CaptureState(WebApplicationFactory<Program> app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return JsonSerializer.Serialize(new {
            Carts = await db.Carts.AsNoTracking().OrderBy(x => x.Id).ToListAsync(),
            Items = await db.CartItems.AsNoTracking().OrderBy(x => x.Id).ToListAsync(),
            Inventory = await db.Inventories.AsNoTracking().OrderBy(x => x.Id).ToListAsync(),
            Discounts = await db.Discounts.AsNoTracking().OrderBy(x => x.Id).ToListAsync(),
            Orders = await db.ParentOrders.CountAsync(), SubOrders = await db.SubOrders.CountAsync(),
            OrderItems = await db.OrderItems.CountAsync(), Payments = await db.Payments.CountAsync(),
            Outbox = await db.OutboxMessages.CountAsync(), Histories = await db.InventoryHistories.CountAsync(),
            Usage = await db.DiscountUsages.CountAsync()
        });
    }
}
