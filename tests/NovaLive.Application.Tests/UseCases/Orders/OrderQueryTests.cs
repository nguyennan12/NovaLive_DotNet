using FluentAssertions;
using Moq;
using NovaLive.Application.UseCases.Orders.Queries.GetMyOrders;
using NovaLive.Application.UseCases.Orders.Queries.GetOrderDetail;
using NovaLive.Application.UseCases.Orders.Queries.GetSellerOrders;
using NovaLive.Application.UseCases.Orders.Queries.GetSellerOrderDetail;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
namespace NovaLive.Application.Tests.UseCases.Orders;
public sealed class OrderQueryTests
{
    [Fact]
    public async Task BuyerList_PassesIdentityFilterDatesAndPageToRepository()
    {
        var c = await OrderOperationsTestContext.Create(); var from = c.Parent.CreatedAt.AddDays(-1); var to = from.AddDays(2);
        c.Checkout.Parents.Setup(p => p.GetMinePagedAsync(c.Parent.BuyerId, "Pending", from, to, 2, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<ParentOrder>)[c.Parent], 8L));
        var result = await new GetMyOrdersQueryHandler(c.Checkout.Parents.Object, c.Checkout.Subs.Object, c.Checkout.Draft.User.Object)
            .Handle(new("Pending", from, to, 2, 3), default);
        result.IsSuccess.Should().BeTrue(); result.Value!.Page.Should().Be(2); result.Value.Size.Should().Be(3);
        result.Value.Total.Should().Be(8); result.Value.Items.Should().ContainSingle(i => i.Id == c.Parent.Id);
    }
    [Fact]
    public async Task BuyerDetail_ContainsSnapshotAndTimeline_AndHidesForeignOrders()
    {
        var c = await OrderOperationsTestContext.Create(cod: true);
        var handler = new GetOrderDetailQueryHandler(c.Reader, c.Checkout.Draft.User.Object);
        var result = await handler.Handle(new(c.Parent.Id), default);
        result.IsSuccess.Should().BeTrue(); result.Value!.PaymentMethod.Should().Be("COD");
        result.Value.SubOrders.Should().ContainSingle(); result.Value.SubOrders[0].Items.Should().ContainSingle();
        result.Value.SubOrders[0].Timeline.Select(h => h.ToStatus).Should().Equal("PendingPayment", "Confirmed");
        result.Value.Recipient.RecipientName.Should().Be("Buyer");
        c.Checkout.Draft.User.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        (await handler.Handle(new(c.Parent.Id), default)).Error.Code.Should().Be("Orders.OrderNotFound");
    }
    [Fact]
    public async Task SellerList_IsScopedToOwnedShopAndCarriesFilters()
    {
        var c = await OrderOperationsTestContext.Create(cod: true, shops: 2);
        var own = c.Checkout.SubList.Where(s => s.ShopId == c.HeaderShopId).ToArray();
        c.Checkout.Subs.Setup(s => s.GetSellerPagedAsync(c.HeaderShopId, SubOrderStatus.Confirmed, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<SubOrder>)own, 1L));
        var result = await new GetSellerOrdersQueryHandler(c.Checkout.Subs.Object, c.Access).Handle(new("Confirmed"), default);
        result.Value!.Items.Should().ContainSingle(i => i.ShopId == c.HeaderShopId);
    }
    [Fact]
    public async Task SellerDetail_HidesOtherShops_AndRejectsSpoofedShopHeader()
    {
        var c = await OrderOperationsTestContext.Create(shops: 2);
        var own = c.Checkout.SubList.Single(s => s.ShopId == c.HeaderShopId);
        var foreign = c.Checkout.SubList.Single(s => s.ShopId != c.HeaderShopId);
        var handler = new GetSellerOrderDetailQueryHandler(c.Reader, c.Access);
        var result = await handler.Handle(new(own.Id), default);
        result.Value!.Items.Should().ContainSingle(); result.Value.Timeline.Should().ContainSingle();
        (await handler.Handle(new(foreign.Id), default)).Error.Code.Should().Be("Orders.SubOrderNotFound");
        c.HeaderShopId = foreign.ShopId;
        (await handler.Handle(new(foreign.Id), default)).Error.Code.Should().Be("Orders.SubOrderNotFound");
    }
    [Fact]
    public void Validators_RejectInvalidPaginationStatusDateRangesAndIds()
    {
        new GetMyOrdersQueryValidator().Validate(new GetMyOrdersQuery("invalid", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(-1), 0, 101))
            .Errors.Should().HaveCount(4);
        new GetSellerOrdersQueryValidator().Validate(new GetSellerOrdersQuery("invalid", null, null, 0, 101))
            .Errors.Should().HaveCount(3);
        new GetOrderDetailQueryValidator().Validate(new GetOrderDetailQuery(Guid.Empty)).IsValid.Should().BeFalse();
        new GetSellerOrderDetailQueryValidator().Validate(new GetSellerOrderDetailQuery(Guid.Empty)).IsValid.Should().BeFalse();
    }
}
