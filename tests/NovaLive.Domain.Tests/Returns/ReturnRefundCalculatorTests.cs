using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;

namespace NovaLive.Domain.Tests.Returns;

public sealed class ReturnRefundCalculatorTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-17T00:00:00Z");

    private static OrderItem[] Price(decimal voucher, params (int Quantity, decimal UnitPrice)[] lines)
    {
        var shopId = Guid.NewGuid();
        var input = lines.Select(l => new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, l.Quantity, l.UnitPrice)).ToArray();
        var discount = voucher == 0 ? null : new VoucherSnapshot(Guid.NewGuid(), shopId, "RETURN-DISCOUNT",
            DiscountType.FixedCart, voucher, 0, null, DiscountAppliesTo.AllProducts, null, Now.AddDays(-1), Now.AddDays(1), true);
        var pricing = PriceCalculator.Calculate(new([new(shopId, 25000, input, discount)], null, null, Now));
        pricing.IsSuccess.Should().BeTrue();
        return pricing.Value!.ShopResults.Single().Items.Select(i => OrderItem.Create(Guid.NewGuid(), "{}", i.UnitPrice + 50000, i)).ToArray();
    }

    [Fact]
    public void Voucher_FullReturnRefundsNetLineTotalFromPriceCalculator()
    {
        var item = Price(10000, (1, 100000)).Single();
        item.UnitPrice.Should().Be(100000); item.DiscountAmount.Should().Be(10000); item.LineTotal.Should().Be(90000);
        ReturnRefundCalculator.Calculate(item, item.Quantity).Value.Should().Be(item.LineTotal);
    }

    [Theory]
    [InlineData(1, 33333)]
    [InlineData(2, 66666)]
    [InlineData(3, 100000)]
    public void Voucher_PartialReturnUsesNetProportionAndPricingFloor(int quantity, decimal expected)
    {
        var item = Price(20000, (3, 40000)).Single();
        item.LineTotal.Should().Be(100000);
        ReturnRefundCalculator.Calculate(item, quantity).Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void NoVoucher_RefundEqualsSnapshotUnitPriceTimesReturnedQuantity(int quantity)
    {
        var item = Price(0, (3, 100000)).Single();
        ReturnRefundCalculator.Calculate(item, quantity).Value.Should().Be(item.UnitPrice * quantity);
    }

    [Theory]
    [InlineData(100000, 33334, 2)]
    [InlineData(1, 1, 2)]
    public void MultiplePartialReturns_FinalReturnTakesRemainderAndTotalNeverExceedsLine(decimal paid, decimal unitPrice, decimal voucher)
    {
        var item = Price(voucher, (3, unitPrice)).Single();
        item.LineTotal.Should().Be(paid);
        var refunds = new List<decimal>();
        for (var previous = 0; previous < item.Quantity; previous++)
        {
            var refund = ReturnRefundCalculator.Calculate(item, 1, previous);
            refund.IsSuccess.Should().BeTrue(); refunds.Add(refund.Value);
            refunds.Sum().Should().BeLessOrEqualTo(item.LineTotal);
        }
        refunds[0].Should().Be(Math.Floor(paid / 3));
        refunds[^1].Should().Be(item.LineTotal - refunds.Take(refunds.Count - 1).Sum());
        refunds.Sum().Should().Be(item.LineTotal);
        ReturnRefundCalculator.Calculate(item, 1, item.Quantity).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void MultipleItems_RefundSumEqualsNetGoodsExcludingShipping()
    {
        var items = Price(10001, (3, 40000), (2, 25000));
        var refunds = items.Select(i => ReturnRefundCalculator.Calculate(i, i.Quantity).Value).ToArray();
        items.Sum(i => i.DiscountAmount).Should().Be(10001);
        refunds.Sum().Should().Be(items.Sum(i => i.LineTotal)).And.Be(159999);
    }

    [Fact]
    public void DiscountGreaterThanNetAmount_IsNotDeductedAgain()
    {
        var item = Price(90000, (1, 100000)).Single();
        ReturnRefundCalculator.Calculate(item, 1).Value.Should().Be(10000);
    }

    [Fact]
    public void FullDiscount_RefundsZero()
    {
        var item = Price(100000, (1, 100000)).Single();
        ReturnRefundCalculator.Calculate(item, 1).Value.Should().Be(0);
    }

    [Fact]
    public void InvalidQuantity_ReturnsBusinessErrorsWithoutOverflow()
    {
        var item = Price(10000, (1, 100000)).Single();
        ReturnRefundCalculator.Calculate(item, 0).Error.Should().Be(ReturnStateErrors.InvalidQuantity);
        ReturnRefundCalculator.Calculate(item, 1, long.MaxValue).Error.Should().Be(ReturnStateErrors.InvalidQuantity);
    }
}
