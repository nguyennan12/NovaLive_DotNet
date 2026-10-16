using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;
using Xunit;

namespace NovaLive.Domain.Tests.FlashSales;

public class FlashSaleItemTests
{
    private readonly Guid _campaignId = Guid.NewGuid();
    private readonly Guid _skuId = Guid.NewGuid();
    private readonly Guid _shopId = Guid.NewGuid();

    [Fact]
    public void Constructor_ValidInput_ShouldCreatePendingItem()
    {
        var item = new FlashSaleItem(_campaignId, _skuId, _shopId, 90_000m, 50, 2);

        item.CampaignId.Should().Be(_campaignId);
        item.SkuId.Should().Be(_skuId);
        item.ShopId.Should().Be(_shopId);
        item.FlashPrice.Should().Be(90_000m);
        item.Quantity.Should().Be(50);
        item.PerUserLimit.Should().Be(2);
        item.ReservedQty.Should().Be(0);
        item.SoldQty.Should().Be(0);
        item.Status.Should().Be(FlashSaleItemStatus.Pending);
        item.RejectionReason.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Constructor_InvalidFlashPrice_ShouldThrow(decimal invalidPrice)
    {
        var act = () => new FlashSaleItem(_campaignId, _skuId, _shopId, invalidPrice, 50, 2);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_InvalidQuantity_ShouldThrow(int invalidQty)
    {
        var act = () => new FlashSaleItem(_campaignId, _skuId, _shopId, 100_000m, invalidQty, 2);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidPerUserLimit_ShouldThrow(int invalidLimit)
    {
        var act = () => new FlashSaleItem(_campaignId, _skuId, _shopId, 100_000m, 50, invalidLimit);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Approve_WhenPending_ShouldSetStatusToApproved()
    {
        var item = new FlashSaleItem(_campaignId, _skuId, _shopId, 90_000m, 50, 2);

        item.Approve();

        item.Status.Should().Be(FlashSaleItemStatus.Approved);
        item.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void Approve_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        var item = new FlashSaleItem(_campaignId, _skuId, _shopId, 90_000m, 50, 2);
        item.Approve();

        var act = () => item.Approve();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_WhenPending_ShouldSetStatusToRejectedAndRecordReason()
    {
        var item = new FlashSaleItem(_campaignId, _skuId, _shopId, 90_000m, 50, 2);

        item.Reject("Không đủ điều kiện giảm giá.");

        item.Status.Should().Be(FlashSaleItemStatus.Rejected);
        item.RejectionReason.Should().Be("Không đủ điều kiện giảm giá.");
    }

    [Fact]
    public void Reject_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        var item = new FlashSaleItem(_campaignId, _skuId, _shopId, 90_000m, 50, 2);
        item.Reject("Lý do");

        var act = () => item.Reject("Lý do khác");

        act.Should().Throw<InvalidOperationException>();
    }
}
