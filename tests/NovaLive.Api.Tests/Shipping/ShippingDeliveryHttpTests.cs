using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NovaLive.Api.Tests.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Shipping;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Api.Tests.Shipping;

[Collection("API host lifecycle")]
public sealed class ShippingDeliveryHttpTests
{
    private const string Path = "/api/v1/shipping/webhook/ghn";
    private const string Secret = "shipping-test-secret";

    private static async Task Seed(WebApplicationFactory<Program> app, DateTimeOffset now)
    {
        using var scope = app.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var buyer = new User("shipping@example.com", "hash", "Buyer", null);
        var shop = new Shop(buyer.Id, "Shop", "shipping-shop");
        var parent = ParentOrder.Create(buyer.Id, "PARENT-1", "{}", 100, 10, 0, 110, null, now.AddDays(-5));
        var sub = SubOrder.Create(parent.Id, "SUB-1", Guid.NewGuid(),
            new ShopCalculationResult(shop.Id, 100, 10, 0, 0, 0, 100, 110, []), true, now.AddDays(-5));
        sub.MarkAsShipping(now.AddDays(-4));
        var shipment = new ShippingOrder(sub.Id, ShippingProvider.GHN, "2", "{}", "{}", 110, 10, 100,
            "TRACK-1", createdAt: now.AddDays(-4));
        db.Users.Add(buyer); db.Shops.Add(shop); db.ParentOrders.Add(parent); db.SubOrders.Add(sub); db.ShippingOrders.Add(shipment);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task AnonymousSignedRawBody_DeliversExactlyOnce_AndRejectsRegression()
    {
        using var app = new AuthApplicationFactory(settings: new() { ["Ghn:WebhookSecret"] = Secret });
        using var client = app.CreateClient();
        app.Clock.UtcNow = DateTimeOffset.Parse("2026-10-17T00:00:00Z");
        await Seed(app, app.Clock.UtcNow);
        // Whitespace/property order differ from serialization: authentication must use original bytes.
        const string raw = """{ "Status": "delivered", "UpdatedAt": "2026-10-16T20:00:00+07:00", "OrderCode": "TRACK-1" }""";
        var signature = Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(Secret), Encoding.UTF8.GetBytes(raw)));
        client.DefaultRequestHeaders.Add("X-Ghn-Signature", signature);
        (await client.PostAsync(Path, new StringContent(raw, Encoding.UTF8, "application/json"))).StatusCode.Should().Be(HttpStatusCode.OK);
        app.Clock.UtcNow = app.Clock.UtcNow.AddHours(2);
        (await client.PostAsync(Path, new StringContent(raw, Encoding.UTF8, "application/json"))).StatusCode.Should().Be(HttpStatusCode.OK);
        client.DefaultRequestHeaders.Remove("X-Ghn-Signature"); client.DefaultRequestHeaders.Add("X-Webhook-Secret", Secret);
        (await client.PostAsJsonAsync(Path, new GhnWebhookRequest("TRACK-1", "picking"))).StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = app.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sub = await db.SubOrders.SingleAsync(); sub.Status.Should().Be(SubOrderStatus.Delivered);
        sub.DeliveredAt.Should().Be(app.Clock.UtcNow.AddHours(-2));
        (await db.ShippingOrders.SingleAsync()).Status.Should().Be(ShippingStatus.Delivered);
        var history = await db.OrderStatusHistories.SingleAsync(); history.ChangedByRole.Should().Be(OrderRole.System);
        history.ChangedBy.Should().BeNull(); (await db.OutboxMessages.SingleAsync()).EventType.Should().Be(nameof(ShipmentDeliveredEvent));
    }

    [Fact]
    public async Task DeliveryOutboxFailure_RollsBackAllStatusAndHistoryChanges()
    {
        var outbox = new Mock<IOutboxRepository>();
        outbox.Setup(o => o.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Injected outbox failure"));
        using var factory = new AuthApplicationFactory(settings: new() { ["Ghn:WebhookSecret"] = Secret });
        using var app = factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        { s.RemoveAll<IOutboxRepository>(); s.AddSingleton(outbox.Object); }));
        using var client = app.CreateClient(); await Seed(app, factory.Clock.UtcNow);
        client.DefaultRequestHeaders.Add("X-Webhook-Secret", Secret);
        (await client.PostAsJsonAsync(Path, new GhnWebhookRequest("TRACK-1", "delivered"))).StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        using var scope = app.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sub = await db.SubOrders.SingleAsync(); sub.Status.Should().Be(SubOrderStatus.Shipping); sub.DeliveredAt.Should().BeNull();
        (await db.ShippingOrders.SingleAsync()).Status.Should().Be(ShippingStatus.ReadyToPick);
        (await db.OrderStatusHistories.CountAsync()).Should().Be(0); (await db.OutboxMessages.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task InvalidAuthenticationAndUnknownState_CannotChangeShipment()
    {
        using var app = new AuthApplicationFactory(settings: new() { ["Ghn:WebhookSecret"] = Secret });
        using var client = app.CreateClient(); await Seed(app, app.Clock.UtcNow);
        (await client.PostAsync(Path, new StringContent("malformed", Encoding.UTF8, "application/json")))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        client.DefaultRequestHeaders.Add("X-Webhook-Secret", Secret);
        (await client.PostAsJsonAsync(Path, new GhnWebhookRequest("TRACK-1", "unknown"))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PostAsJsonAsync(Path, new GhnWebhookRequest("missing", "delivered"))).StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var scope = app.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.ShippingOrders.SingleAsync()).Status.Should().Be(ShippingStatus.ReadyToPick);
        (await db.OutboxMessages.CountAsync()).Should().Be(0);
    }
}
