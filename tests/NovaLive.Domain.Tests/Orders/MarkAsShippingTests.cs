using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;

namespace NovaLive.Domain.Tests.Orders;

public sealed class MarkAsShippingTests
{
    private static SubOrder Create() => SubOrder.Create(Guid.NewGuid(), "ORDER-S1", Guid.NewGuid(),
        new ShopCalculationResult(Guid.NewGuid(), 100, 30, 0, 0, 0, 100, 130, []), true, DateTimeOffset.UtcNow);

    [Fact]
    public void Confirmed_AdvancesWithUtcTime()
    {
        var sub = Create(); var now = new DateTimeOffset(2026, 10, 16, 0, 0, 0, TimeSpan.Zero);
        sub.MarkAsShipping(now).IsSuccess.Should().BeTrue();
        sub.Status.Should().Be(SubOrderStatus.Shipping); sub.ShippedAt.Should().Be(now); sub.UpdatedAt.Should().Be(now);
    }

    [Theory]
    [InlineData(SubOrderStatus.PendingPayment)]
    [InlineData(SubOrderStatus.Processing)]
    [InlineData(SubOrderStatus.Shipping)]
    [InlineData(SubOrderStatus.Delivered)]
    [InlineData(SubOrderStatus.Completed)]
    [InlineData(SubOrderStatus.Cancelled)]
    [InlineData(SubOrderStatus.ReturnRequested)]
    [InlineData(SubOrderStatus.Returned)]
    [InlineData(SubOrderStatus.Refunded)]
    public void InvalidTransition_IsFailureWithoutMutation(SubOrderStatus status)
    {
        var sub = Create();
        typeof(SubOrder).GetProperty(nameof(SubOrder.Status))!.SetValue(sub, status);
        sub.MarkAsShipping(DateTimeOffset.UtcNow).Error.Should().Be(OrderStateErrors.InvalidStatusTransition);
        sub.Status.Should().Be(status); sub.ShippedAt.Should().BeNull();
    }
}
