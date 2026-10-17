using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
namespace NovaLive.Domain.Tests.Returns;
public sealed class ReturnRequestTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-17T00:00:00Z");
    private static SubOrder Sub() => SubOrder.Create(Guid.NewGuid(), "SUB", Guid.NewGuid(),
        new ShopCalculationResult(Guid.NewGuid(), 100, 10, 0, 0, 0, 100, 110, []), true, Now);
    [Fact]
    public void ReturnWindow_IncludesSeventhDayButRejectsLater()
    {
        var sub = Sub(); sub.MarkAsShipping(Now); sub.MarkAsDelivered(Now);
        sub.RequestReturn(Now.AddDays(7).AddTicks(1)).Error.Should().Be(ReturnStateErrors.Expired);
        sub.Status.Should().Be(SubOrderStatus.Delivered);
        sub.RequestReturn(Now.AddDays(7)).IsSuccess.Should().BeTrue();
        sub.Status.Should().Be(SubOrderStatus.ReturnRequested);
        sub.RequestReturn(Now.AddDays(7)).Error.Should().Be(ReturnStateErrors.NotDelivered);
        Sub().RequestReturn(Now).Error.Should().Be(ReturnStateErrors.NotDelivered);
    }
    [Theory]
    [InlineData(1, 0, 90)]
    [InlineData(2, 0, 180)]
    [InlineData(2, 2, 180)]
    [InlineData(4, 0, 360)]
    public void Refund_UsesNetSnapshotAndReturnedQuantity(int quantity, int previous, decimal expected)
    {
        var item = OrderItem.Create(Guid.NewGuid(), "{}", 100, new(Guid.NewGuid(), Guid.NewGuid(), 4, 100, 400, 0, 40, 40, 360));
        ReturnRefundCalculator.Calculate(item, quantity, previous).Value.Should().Be(expected);
    }
    [Fact]
    public void Refund_CumulativeRoundingCannotOverRefund()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "{}", 1, new(Guid.NewGuid(), Guid.NewGuid(), 3, 1, 1, 0, 0, 0, 1));
        Enumerable.Range(0, 3).Sum(i => ReturnRefundCalculator.Calculate(item, 1, i).Value).Should().Be(1);
        ReturnRefundCalculator.Calculate(item, 2, 2).Error.Should().Be(ReturnStateErrors.InvalidQuantity);
        ReturnRefundCalculator.Calculate(item, 0).IsFailure.Should().BeTrue();
    }
    [Fact]
    public void InvalidSnapshot_ReturnsBusinessError()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "{}", 1, new(Guid.NewGuid(), Guid.NewGuid(), 1, 1, 1, 0, 2, 2, -1));
        ReturnRefundCalculator.Calculate(item, 1).Error.Should().Be(ReturnStateErrors.InvalidSnapshot);
    }
}
