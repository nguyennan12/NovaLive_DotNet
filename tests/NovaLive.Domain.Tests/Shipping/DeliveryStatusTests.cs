using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Shipping;

namespace NovaLive.Domain.Tests.Shipping;

public sealed class DeliveryStatusTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-17T00:00:00Z");
    private static ShippingOrder Shipment() => new(Guid.NewGuid(), ShippingProvider.GHN, "2", "{}", "{}", 0, 10, 100,
        "TRACK-1", createdAt: Now.AddDays(-4));
    private static SubOrder Sub(bool cod = true) => SubOrder.Create(Guid.NewGuid(), "SUB-1", Guid.NewGuid(),
        new ShopCalculationResult(Guid.NewGuid(), 100, 10, 0, 0, 0, 100, 110, []), cod, Now.AddDays(-4));

    [Fact]
    public void Transport_AdvancesAndStampsOnlyAcceptedEvents()
    {
        var shipment = Shipment();
        shipment.UpdateStatusFromWebhook(ShippingStatus.Picking, "picking", Now).Should().BeTrue();
        shipment.PickedAt.Should().Be(Now);
        shipment.UpdateStatusFromWebhook(ShippingStatus.Delivering, "delivering", Now.AddHours(1)).Should().BeTrue();
        shipment.UpdateStatusFromWebhook(ShippingStatus.Picking, "old", Now.AddHours(2)).Should().BeFalse();
        shipment.UpdateStatusFromWebhook(ShippingStatus.Delivering, "duplicate", Now.AddHours(3)).Should().BeFalse();
        shipment.Status.Should().Be(ShippingStatus.Delivering); shipment.WebhookPayload.Should().Be("delivering");
        shipment.UpdatedAt.Should().Be(Now.AddHours(1)); shipment.PickedAt.Should().Be(Now);
    }

    [Theory]
    [InlineData(ShippingStatus.Returned)]
    [InlineData(ShippingStatus.Cancelled)]
    public void TerminalShipment_RejectsEveryLaterEvent(ShippingStatus terminal)
    {
        var shipment = Shipment();
        shipment.UpdateStatusFromWebhook(terminal, "terminal", Now).Should().BeTrue();
        foreach (var status in Enum.GetValues<ShippingStatus>())
            shipment.UpdateStatusFromWebhook(status, "late", Now.AddDays(1)).Should().BeFalse();
        shipment.Status.Should().Be(terminal); shipment.UpdatedAt.Should().Be(Now);
        shipment.WebhookPayload.Should().Be("terminal");
        shipment.DeliveredAt.Should().Be(terminal == ShippingStatus.Delivered ? Now : null);
    }

    [Fact]
    public void FailedShipment_CannotRegressToTransportOrDelivered()
    {
        var shipment = Shipment();
        shipment.UpdateStatusFromWebhook(ShippingStatus.Failed, "{}", Now).Should().BeTrue();
        shipment.UpdateStatusFromWebhook(ShippingStatus.Delivering, "{}", Now.AddHours(1)).Should().BeFalse();
        shipment.UpdateStatusFromWebhook(ShippingStatus.Delivered, "{}", Now.AddHours(2)).Should().BeFalse();
        shipment.DeliveredAt.Should().BeNull();
    }

    [Theory]
    [InlineData(ShippingStatus.Failed)]
    [InlineData(ShippingStatus.Returned)]
    [InlineData(ShippingStatus.Cancelled)]
    public void DeliveredShipment_AcceptsCarrierExceptionsWithoutChangingDeliveryTime(ShippingStatus status)
    {
        var shipment = Shipment();
        shipment.UpdateStatusFromWebhook(ShippingStatus.Delivered, "delivered", Now).Should().BeTrue();
        shipment.UpdateStatusFromWebhook(ShippingStatus.Picking, "old", Now.AddHours(1)).Should().BeFalse();
        shipment.UpdateStatusFromWebhook(ShippingStatus.Delivered, "duplicate", Now.AddHours(1)).Should().BeFalse();
        shipment.UpdateStatusFromWebhook(status, "exception", Now.AddHours(2)).Should().BeTrue();
        shipment.UpdateStatusFromWebhook(ShippingStatus.Delivered, "late", Now.AddHours(3)).Should().BeFalse();
        shipment.Status.Should().Be(status); shipment.DeliveredAt.Should().Be(Now);
    }

    [Fact]
    public void InvalidShipmentStatus_DoesNotChangeAnything()
    {
        var shipment = Shipment();
        shipment.UpdateStatusFromWebhook((ShippingStatus)99, "invalid", Now).Should().BeFalse();
        shipment.Status.Should().Be(ShippingStatus.ReadyToPick); shipment.WebhookPayload.Should().BeNull();
        shipment.UpdatedAt.Should().Be(Now.AddDays(-4));
    }

    [Fact]
    public void ShippingSubOrder_DeliversOnlyOnce()
    {
        var sub = Sub(); sub.MarkAsShipping(Now.AddDays(-3)).IsSuccess.Should().BeTrue();
        sub.MarkAsDelivered(Now).Should().BeTrue(); sub.MarkAsDelivered(Now.AddDays(1)).Should().BeFalse();
        sub.Status.Should().Be(SubOrderStatus.Delivered); sub.DeliveredAt.Should().Be(Now); sub.UpdatedAt.Should().Be(Now);
    }

    [Theory]
    [InlineData(SubOrderStatus.PendingPayment)]
    [InlineData(SubOrderStatus.Confirmed)]
    [InlineData(SubOrderStatus.Processing)]
    [InlineData(SubOrderStatus.Completed)]
    [InlineData(SubOrderStatus.Cancelled)]
    [InlineData(SubOrderStatus.ReturnRequested)]
    [InlineData(SubOrderStatus.Returned)]
    [InlineData(SubOrderStatus.Refunded)]
    public void OtherSubOrderStates_CannotBeOverwrittenByDelivery(SubOrderStatus state)
    {
        var sub = Sub();
        typeof(SubOrder).GetProperty(nameof(SubOrder.Status))!.SetValue(sub, state);
        sub.MarkAsDelivered(Now).Should().BeFalse(); sub.Status.Should().Be(state); sub.DeliveredAt.Should().BeNull();
    }
}
