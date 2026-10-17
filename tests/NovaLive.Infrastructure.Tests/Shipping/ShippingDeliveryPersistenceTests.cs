using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Shipping;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Infrastructure.Shipping;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.Shipping;

public sealed class ShippingDeliveryPersistenceTests
{
    [Fact]
    public async Task RealRepositoryAndPolling_OnlyDeliverStaleInTransitShippingSubOrders()
    {
        var provider = new Mock<IShippingProvider>();
        provider.SetupGet(p => p.ProviderCode).Returns("GHN");
        provider.Setup(p => p.GetTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingTrackingResult("delivered", []));
        await using var host = new AuthTestHost();
        await host.InitializeAsync(s =>
        {
            s.AddScoped<IShippingOrderRepository, ShippingOrderRepository>();
            s.AddScoped<ISubOrderRepository, SubOrderRepository>();
            s.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
            s.AddScoped<IOutboxRepository, OutboxRepository>();
            s.AddSingleton(provider.Object); s.AddScoped<IShippingProviderResolver, ShippingProviderResolver>();
        });
        await host.WithDb(async db =>
        {
            var user = new User("shipping@example.com", "hash", "Buyer", null);
            var shop = new Shop(user.Id, "Shipping shop", "shipping-shop");
            var parent = ParentOrder.Create(user.Id, "PARENT-1", "{}", 600, 60, 0, 660, null, host.Clock.UtcNow.AddDays(-6));
            db.Users.Add(user); db.Shops.Add(shop); db.ParentOrders.Add(parent);
            var scenarios = new[] { (4, ShippingStatus.Picking, true), (5, ShippingStatus.Delivering, true),
                (2, ShippingStatus.Delivering, true), (3, ShippingStatus.Picking, true),
                (4, ShippingStatus.ReadyToPick, true), (4, ShippingStatus.Delivering, false) };
            for (var index = 0; index < scenarios.Length; index++)
            {
                var (days, status, shippingSub) = scenarios[index];
                var sub = SubOrder.Create(parent.Id, "SUB-" + index, Guid.NewGuid(),
                    new ShopCalculationResult(shop.Id, 100, 10, 0, 0, 0, 100, 110, []), true, host.Clock.UtcNow.AddDays(-6));
                if (shippingSub) sub.MarkAsShipping(host.Clock.UtcNow.AddDays(-5));
                var shipment = new ShippingOrder(sub.Id, ShippingProvider.GHN, "2", "{}", "{}", 0, 10, 100,
                    "TRACK-" + index, createdAt: host.Clock.UtcNow.AddDays(-days));
                shipment.UpdateStatusFromWebhook(status, "{}", host.Clock.UtcNow.AddDays(-days));
                db.SubOrders.Add(sub); db.ShippingOrders.Add(shipment);
            }
            await db.SaveChangesAsync();
        });
        using var worker = new ShippingStatusPollingWorker(host.Services.GetRequiredService<IServiceScopeFactory>(), host.Clock,
            NullLogger<ShippingStatusPollingWorker>.Instance);
        await worker.RunOnceAsync(default); await worker.RunOnceAsync(default);
        provider.Verify(p => p.GetTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        await host.WithDb(async db =>
        {
            (await db.SubOrders.Where(s => s.Status == SubOrderStatus.Delivered).CountAsync()).Should().Be(2);
            (await db.OrderStatusHistories.CountAsync()).Should().Be(2); (await db.OutboxMessages.CountAsync()).Should().Be(2);
            (await db.ShippingOrders.Where(s => s.Status == ShippingStatus.Delivered).CountAsync()).Should().Be(2);
        });
    }
}
