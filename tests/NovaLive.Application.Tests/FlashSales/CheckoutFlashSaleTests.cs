using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Tests.UseCases.Orders;
using NovaLive.Application.UseCases.FlashSales;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;

namespace NovaLive.Application.Tests.FlashSales;

public sealed class CheckoutFlashSaleTests
{
    private static (CheckoutCommandTestContext Context, FlashSaleItem Item, FlashSaleCampaign Campaign) Create(int limit = 3)
    {
        var c = new CheckoutCommandTestContext();
        var campaign = new FlashSaleCampaign("Active", c.Draft.Now.AddHours(-1), c.Draft.Now.AddHours(1));
        typeof(FlashSaleCampaign).GetProperty(nameof(FlashSaleCampaign.Status))!.SetValue(campaign, FlashSaleCampaignStatus.Active);
        var sku = c.Draft.SkuList.Single();
        var item = new FlashSaleItem(campaign.Id, sku.Id, sku.ShopId, 80m, 10, limit); item.Approve();
        c.Draft.FlashSales.Setup(f => f.GetActiveItemsBySkuIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { item });
        c.Draft.FlashSales.Setup(f => f.GetCampaignByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        c.Draft.FlashSales.Setup(f => f.GetItemByIdAsync(item.Id, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        c.Draft.FlashSales.Setup(f => f.TryReserveAsync(item.Id, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        return (c, item, campaign);
    }

    [Fact]
    public async Task Success_UsesLiveFlashPrice_ReservesBothStocks_AndMarksSourceAndLine()
    {
        var (c, item, _) = Create();
        var result = await c.Run();
        result.IsSuccess.Should().BeTrue(); c.Committed.Should().BeTrue();
        c.ItemList.Single().UnitPrice.Should().Be(80m);
        c.ItemList.Single().FlashSaleItemId.Should().Be(item.Id);
        c.ItemList.Single().FlashSaleState.Should().Be(FlashSaleReservationState.Reserved);
        c.SubList.Single().OrderSource.Should().Be(OrderSource.FlashSale);
        c.Draft.Stocks[item.SkuId].ReservedQty.Should().Be(2);
        c.Draft.FlashSales.Verify(f => f.LockBuyerItemAsync(c.Draft.UserId, item.Id, It.IsAny<CancellationToken>()), Times.Once);
        c.Draft.FlashSales.Verify(f => f.TryReserveAsync(item.Id, 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoldOut_ReturnsResult_WithoutCreatingOrders()
    {
        var (c, item, _) = Create();
        c.Draft.FlashSales.Setup(f => f.TryReserveAsync(item.Id, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        (await c.Run()).Error.Should().Be(FlashSalesErrors.SoldOut);
        c.ParentList.Should().BeEmpty(); c.ItemList.Should().BeEmpty(); c.Committed.Should().BeFalse();
    }

    [Fact]
    public async Task UsagePlusRequestedQuantity_ExceedsLimit_BeforeReserve()
    {
        var (c, item, _) = Create();
        c.Items.Setup(o => o.GetFlashSaleUsageAsync(c.Draft.UserId, item.Id, It.IsAny<CancellationToken>())).ReturnsAsync(2L);
        (await c.Run()).Error.Should().Be(FlashSalesErrors.PerUserLimitExceeded);
        c.ParentList.Should().BeEmpty();
        c.Draft.FlashSales.Verify(f => f.TryReserveAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExactLimit_IsAllowed()
    {
        var (c, item, _) = Create();
        c.Items.Setup(o => o.GetFlashSaleUsageAsync(c.Draft.UserId, item.Id, It.IsAny<CancellationToken>())).ReturnsAsync(1L);
        (await c.Run()).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MixedShopOrder_OnlyFlashLineGetsFlashPriceAndReservation()
    {
        var (c, flash, _) = Create(); var sku = c.Draft.SkuList.Single();
        var regular = new NovaLive.Domain.Products.Sku(sku.SpuId, sku.ShopId, "REGULAR", "{}", 200m, 150m);
        c.Draft.SkuList.Add(regular);
        c.Draft.Stocks.Add(regular.Id, new NovaLive.Domain.Inventory.Inventory(regular.Id, sku.ShopId, 10));
        c.Draft.Items.Add(new NovaLive.Domain.Carts.CartItem(Guid.NewGuid(), c.Draft.Cart.Id, regular.Id, sku.ShopId, 1, 1m, c.Draft.Now));
        (await c.Run()).IsSuccess.Should().BeTrue();
        c.ItemList.Single(i => i.SkuId == flash.SkuId).UnitPrice.Should().Be(80m);
        var regularLine = c.ItemList.Single(i => i.SkuId == regular.Id);
        regularLine.UnitPrice.Should().Be(150m); regularLine.FlashSaleItemId.Should().BeNull(); regularLine.FlashSaleState.Should().BeNull();
        c.SubList.Should().HaveCount(1); c.SubList.Single().OrderSource.Should().Be(OrderSource.FlashSale);
    }

    [Fact]
    public async Task CampaignCancelledAfterPreview_FailsCheckout()
    {
        var (c, _, campaign) = Create(); campaign.Cancel();
        (await c.Run()).Error.Should().Be(FlashSalesErrors.CampaignNotActive);
        c.ParentList.Should().BeEmpty(); c.Committed.Should().BeFalse();
    }

    [Fact]
    public async Task ItemWithdrawnAfterPreview_FailsCheckout()
    {
        var (c, item, _) = Create();
        c.Draft.FlashSales.Setup(f => f.GetItemByIdAsync(item.Id, It.IsAny<CancellationToken>())).ReturnsAsync((FlashSaleItem?)null);
        (await c.Run()).Error.Should().Be(FlashSalesErrors.ItemNotAvailable);
        c.ParentList.Should().BeEmpty();
    }

    [Fact]
    public async Task Cod_KeepsQuotaReservedAndPaymentPending()
    {
        var (c, _, _) = Create();
        (await c.Run(c.Request with { PaymentMethod = "COD" })).IsSuccess.Should().BeTrue();
        c.SubList.Single().Status.Should().Be(SubOrderStatus.Confirmed);
        c.PaymentList.Single().Status.Should().Be(PaymentStatus.Pending);
        c.ItemList.Single().FlashSaleState.Should().Be(FlashSaleReservationState.Reserved);
        c.Draft.FlashSales.Verify(f => f.ConfirmSoldAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MultipleFlashItems_AcquireLocksAndReserveInIdOrder()
    {
        var (c, item, campaign) = Create(); c.Draft.AddShop();
        var sku = c.Draft.SkuList.Last();
        var other = new FlashSaleItem(campaign.Id, sku.Id, sku.ShopId, 70, 10, 3); other.Approve();
        c.Draft.FlashSales.Setup(f => f.GetActiveItemsBySkuIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { other, item });
        c.Draft.FlashSales.Setup(f => f.GetItemByIdAsync(other.Id, It.IsAny<CancellationToken>())).ReturnsAsync(other);
        c.Draft.FlashSales.Setup(f => f.TryReserveAsync(other.Id, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        (await c.Run()).IsSuccess.Should().BeTrue();
        foreach (var method in new[] { nameof(IFlashSaleRepository.LockBuyerItemAsync), nameof(IFlashSaleRepository.TryReserveAsync) })
        {
            var offset = method == nameof(IFlashSaleRepository.LockBuyerItemAsync) ? 1 : 0;
            c.Draft.FlashSales.Invocations.Where(i => i.Method.Name == method).Select(i => (Guid)i.Arguments[offset])
                .Should().Equal(new[] { item.Id, other.Id }.Order());
        }
    }
}
