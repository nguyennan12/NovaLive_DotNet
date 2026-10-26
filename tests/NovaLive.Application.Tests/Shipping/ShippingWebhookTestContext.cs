using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Shipping.Commands.Webhook;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Shipping;
using NovaLive.Domain.System;

namespace NovaLive.Application.Tests.Shipping;

public sealed class ShippingWebhookTestContext
{
    public DateTimeOffset Now { get; } = DateTimeOffset.Parse("2026-10-17T10:00:00Z");
    public Mock<IShippingOrderRepository> Shipments { get; } = new();
    public Mock<ISubOrderRepository> Subs { get; } = new();
    public Mock<IOrderStatusHistoryRepository> Histories { get; } = new();
    public Mock<IOutboxRepository> Outbox { get; } = new();
    public Mock<IDateTimeProvider> Clock { get; } = new();
    public List<OrderStatusHistory> HistoryList { get; } = [];
    public List<OutboxMessage> Messages { get; } = [];
    public ShippingOrder Shipment { get; }
    public SubOrder Sub { get; }
    public HandleShippingWebhookCommandHandler Handler => new(Shipments.Object, Subs.Object, Histories.Object,
        Outbox.Object, Clock.Object, NullLogger<HandleShippingWebhookCommandHandler>.Instance);
    public HandleShippingWebhookCommand Command(ShippingStatus status = ShippingStatus.Delivered) =>
        new(ShippingProvider.GHN, "TRACK-1", status, "{}", Now.AddHours(-1));

    public ShippingWebhookTestContext()
    {
        Clock.SetupGet(c => c.UtcNow).Returns(() => Now);
        Sub = SubOrder.Create(Guid.NewGuid(), "SUB-1", Guid.NewGuid(),
            new ShopCalculationResult(Guid.NewGuid(), 100, 10, 0, 0, 0, 100, 110, []), true, Now.AddDays(-5));
        Sub.MarkAsShipping(Now.AddDays(-4));
        Shipment = new ShippingOrder(Sub.Id, ShippingProvider.GHN, "2", "{}", "{}", 110, 10, 500,
            "TRACK-1", createdAt: Now.AddDays(-4));
        Shipments.Setup(s => s.GetByTrackingForUpdateAsync(ShippingProvider.GHN, "TRACK-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Shipment);
        Subs.Setup(s => s.GetForUpdateAsync(Sub.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Sub);
        Histories.Setup(h => h.AddAsync(It.IsAny<OrderStatusHistory>(), It.IsAny<CancellationToken>()))
            .Callback((OrderStatusHistory h, CancellationToken _) => HistoryList.Add(h)).Returns(Task.CompletedTask);
        Outbox.Setup(o => o.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Callback((OutboxMessage m, CancellationToken _) => Messages.Add(m)).Returns(Task.CompletedTask);
    }
}
