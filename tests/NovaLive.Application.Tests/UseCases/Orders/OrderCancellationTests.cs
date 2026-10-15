using FluentAssertions;
using NovaLive.Application.UseCases.Orders.Commands.CancelOrder;
using NovaLive.Application.UseCases.Orders.Commands.CancelSubOrder;
using NovaLive.Domain.Common;
namespace NovaLive.Application.Tests.UseCases.Orders;
public sealed class OrderCancellationTests
{
    [Fact]
    public async Task BuyerCancel_ReleasesAllReservationsExpiresPaymentAndCannotRepeat()
    {
        var c = await OrderOperationsTestContext.Create(shops: 2);
        var result = await c.BuyerCancel.Handle(new(c.Parent.Id, new("Changed my mind")), default);
        result.IsSuccess.Should().BeTrue(); c.Parent.PaymentStatus.Should().Be(ParentOrderPaymentStatus.Cancelled);
        c.Checkout.SubList.Should().OnlyContain(s => s.Status == SubOrderStatus.Cancelled && s.CancelledBy == c.Parent.BuyerId);
        c.Checkout.Draft.Stocks.Values.Should().OnlyContain(i => i.ReservedQty == 0 && i.QtyOnHand == 10);
        c.Checkout.PaymentList.Single().Status.Should().Be(PaymentStatus.Expired);
        c.Checkout.Ledger.Count(h => h.ChangeType == InventoryChangeType.ReserveRelease).Should().Be(2);
        c.Checkout.HistoryList.Count(h => h.ChangedByRole == OrderRole.Buyer && h.ToStatus == "Cancelled").Should().Be(2);
        var count = c.Checkout.Ledger.Count;
        (await c.BuyerCancel.Handle(new(c.Parent.Id, new("Again")), default)).Error.Code.Should().Be("Orders.AlreadyCancelled");
        c.Checkout.Ledger.Should().HaveCount(count);
    }
    [Fact]
    public async Task BuyerCannotCancelConfirmedCodOrAnotherBuyersOrder()
    {
        var c = await OrderOperationsTestContext.Create(cod: true);
        (await c.BuyerCancel.Handle(new(c.Parent.Id, new("No")), default)).Error.Code.Should().Be("Orders.CannotCancelConfirmedOrder");
        c.Checkout.Draft.User.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        (await c.BuyerCancel.Handle(new(c.Parent.Id, new("No")), default)).Error.Code.Should().Be("Orders.OrderNotFound");
        c.Checkout.Ledger.Should().ContainSingle(h => h.ChangeType == InventoryChangeType.ReserveAdd);
    }
    [Fact]
    public async Task SellerCancelsOnlyOwnSubOrder_ParentCancelledOnlyAfterLastShop()
    {
        var c = await OrderOperationsTestContext.Create(cod: true, shops: 2);
        var sub = c.Checkout.SubList.Single(s => s.ShopId == c.HeaderShopId);
        (await c.SellerCancel.Handle(new(sub.Id, new("Out of stock")), default)).IsSuccess.Should().BeTrue();
        c.Parent.PaymentStatus.Should().Be(ParentOrderPaymentStatus.Pending);
        c.Checkout.Draft.Stocks.Values.Count(i => i.ReservedQty == 0).Should().Be(1);
        c.Checkout.PaymentList.Single().Status.Should().Be(PaymentStatus.Pending);
        var count = c.Checkout.Ledger.Count;
        (await c.SellerCancel.Handle(new(sub.Id, new("Again")), default)).Error.Code.Should().Be("Orders.AlreadyCancelled");
        c.Checkout.Ledger.Should().HaveCount(count);
        c.OwnedShopIndex = 1; c.HeaderShopId = c.Checkout.Draft.ShopList[1].Id;
        var last = c.Checkout.SubList.Single(s => s.ShopId == c.HeaderShopId);
        (await c.SellerCancel.Handle(new(last.Id, new("Cannot fulfill")), default)).IsSuccess.Should().BeTrue();
        c.Parent.PaymentStatus.Should().Be(ParentOrderPaymentStatus.Cancelled);
        c.Checkout.PaymentList.Single().Status.Should().Be(PaymentStatus.Expired);
        c.Checkout.Draft.Stocks.Values.Should().OnlyContain(i => i.ReservedQty == 0);
    }
    [Theory]
    [InlineData(SubOrderStatus.PendingPayment)]
    [InlineData(SubOrderStatus.Confirmed)]
    [InlineData(SubOrderStatus.Processing)]
    public async Task SellerMayCancelBeforeShipping(SubOrderStatus status)
    {
        var c = await OrderOperationsTestContext.Create(); var sub = c.Checkout.SubList.Single();
        // T32 owns payment transitions; arrange boundary states without adding settlement to this task.
        typeof(NovaLive.Domain.Orders.SubOrder).GetProperty("Status")!.SetValue(sub, status);
        (await c.SellerCancel.Handle(new(sub.Id, new("Issue")), default)).IsSuccess.Should().BeTrue();
    }
    [Fact]
    public async Task PaidOnlineOrShipping_IsBlockedWithoutInventoryRelease()
    {
        var c = await OrderOperationsTestContext.Create(cod: true); var sub = c.Checkout.SubList.Single();
        typeof(NovaLive.Domain.Payments.Payment).GetProperty("Status")!.SetValue(c.Checkout.PaymentList.Single(), PaymentStatus.Success);
        (await c.SellerCancel.Handle(new(sub.Id, new("Issue")), default)).Error.Code.Should().Be("Orders.CannotCancelPaidOrderYet");
        sub.MarkAsShipping();
        typeof(NovaLive.Domain.Payments.Payment).GetProperty("Status")!.SetValue(c.Checkout.PaymentList.Single(), PaymentStatus.Pending);
        (await c.SellerCancel.Handle(new(sub.Id, new("Issue")), default)).Error.Code.Should().Be("Orders.InvalidStatusTransition");
        c.Checkout.Ledger.Should().ContainSingle();
    }
    [Fact]
    public async Task SellerCannotCancelForeignSubOrder()
    {
        var c = await OrderOperationsTestContext.Create(shops: 2);
        var foreign = c.Checkout.SubList.Single(s => s.ShopId != c.HeaderShopId);
        (await c.SellerCancel.Handle(new(foreign.Id, new("Issue")), default)).Error.Code.Should().Be("Orders.SubOrderNotFound");
        c.Checkout.Ledger.Should().HaveCount(2);
    }
    [Fact]
    public void Validators_RequireIdsAndBoundedReasons()
    {
        new CancelOrderCommandValidator().Validate(new CancelOrderCommand(Guid.Empty, new(""))).Errors.Should().HaveCount(2);
        new CancelSubOrderCommandValidator().Validate(new CancelSubOrderCommand(Guid.Empty, new(new string('x', 501)))).Errors.Should().HaveCount(2);
    }
}
