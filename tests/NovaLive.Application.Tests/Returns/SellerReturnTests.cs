using FluentAssertions;
using Moq;
using NovaLive.Application.UseCases.Returns;
using NovaLive.Application.UseCases.Returns.Commands.ApproveReturn;
using NovaLive.Application.UseCases.Returns.Commands.RejectReturn;
using NovaLive.Application.UseCases.Returns.Commands.ConfirmReturnReceived;
using NovaLive.Application.UseCases.Returns.Queries.GetSellerReturns;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;
namespace NovaLive.Application.Tests.Returns;
public sealed class SellerReturnTests
{
    [Fact]
    public async Task Approve_SnapshotsReturnAddressAndFiltersSellerList()
    {
        var c = await SellerReturnTestContext.Create();
        var result = await c.Approve.Handle(new(c.Request.Id, new("Ship here")), default);
        result.IsSuccess.Should().BeTrue(); result.Value!.ReturnAddress!.DetailAddress.Should().Be("Return street");
        c.Request.Status.Should().Be(ReturnStatus.SellerApproved);
        var list = await c.List.Handle(new("SellerApproved"), default); list.Value!.Items.Should().ContainSingle();
        list.Value.Items[0].ReturnAddress!.Id.Should().Be(c.Address.Id);
        (await c.Approve.Handle(new(c.Request.Id, new(null)), default)).Error.Should().Be(ReturnErrors.InvalidStatus);
    }
    [Fact]
    public async Task Reject_PreservesBuyerEvidenceAndSubOrderForDispute()
    {
        var c = await SellerReturnTestContext.Create();
        var result = await c.Reject.Handle(new(c.Request.Id, new("Not eligible", ["https://example.com/seller.jpg"])), default);
        result.Value!.SellerResponse.Should().Be("Not eligible"); result.Value.SellerEvidenceUrls.Should().ContainSingle();
        c.Request.EvidenceUrls.Should().Equal(c.Buyer.Request.EvidenceUrls);
        c.Request.Status.Should().Be(ReturnStatus.SellerRejected); c.Buyer.Sub.Status.Should().Be(SubOrderStatus.ReturnRequested);
        (await c.Reject.Handle(new(c.Request.Id, new("Again", ["https://example.com/seller.jpg"])), default)).Error.Should().Be(ReturnErrors.InvalidStatus);
        (await c.Approve.Handle(new(c.Request.Id, new(null)), default)).Error.Should().Be(ReturnErrors.InvalidStatus);
        c.WalletLedger.Should().BeEmpty(); c.StockLedger.Should().BeEmpty();
    }
    [Fact]
    public async Task ForeignSellerAndSpoofedShop_AreHiddenForAllOperations()
    {
        var c = await SellerReturnTestContext.Create(); c.CurrentShop.SetupGet(s => s.ShopId).Returns(Guid.NewGuid());
        (await c.Approve.Handle(new(c.Request.Id, new(null)), default)).Error.Should().Be(ReturnErrors.NotFound);
        (await c.Reject.Handle(new(c.Request.Id, new("Reason", ["https://example.com/evidence"])), default)).Error.Should().Be(ReturnErrors.NotFound);
        (await c.Received.Handle(new(c.Request.Id), default)).Error.Should().Be(ReturnErrors.NotFound);
        (await c.List.Handle(new(), default)).Error.Should().Be(ReturnErrors.NotFound);
        c.CurrentShop.SetupGet(s => s.ShopId).Returns(c.Shop.Id);
        c.Buyer.User.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        (await c.Approve.Handle(new(c.Request.Id, new(null)), default)).Error.Should().Be(ReturnErrors.NotFound);
        c.Request.Status.Should().Be(ReturnStatus.Pending);
    }
    [Fact]
    public async Task Received_InsufficientBalanceDoesNotChangeAnything()
    {
        var c = await SellerReturnTestContext.Create(true); c.Wallet.DebitRefund(1000, c.Buyer.Now);
        (await c.Received.Handle(new(c.Request.Id), default)).Error.Should().Be(ReturnErrors.InsufficientShopBalance);
        c.Request.Status.Should().Be(ReturnStatus.SellerApproved); c.Buyer.Sub.Status.Should().Be(SubOrderStatus.ReturnRequested);
        c.Wallet.Balance.Should().Be(0); c.Stock.QtyOnHand.Should().Be(5); c.WalletLedger.Should().BeEmpty(); c.StockLedger.Should().BeEmpty();
    }
    [Fact]
    public async Task Received_UpdatesMoneyStockLedgersAndHistoryExactlyOnce()
    {
        var c = await SellerReturnTestContext.Create(true);
        (await c.Received.Handle(new(c.Request.Id), default)).IsSuccess.Should().BeTrue();
        c.Request.Status.Should().Be(ReturnStatus.Completed); c.Request.ResolvedAt.Should().Be(c.Buyer.Now);
        c.Buyer.Sub.Status.Should().Be(SubOrderStatus.Returned); c.Wallet.Balance.Should().Be(820); c.Stock.QtyOnHand.Should().Be(7);
        var money = c.WalletLedger.Should().ContainSingle().Subject;
        money.Type.Should().Be(WalletTxType.OrderRefund); money.Amount.Should().Be(-180);
        money.BalanceBefore.Should().Be(1000); money.BalanceAfter.Should().Be(820);
        money.RefType.Should().Be("OrderReturn"); money.RefId.Should().Be(c.Request.Id); money.CreatedAt.Should().Be(c.Buyer.Now);
        var stock = c.StockLedger.Should().ContainSingle().Subject;
        stock.ChangeType.Should().Be(InventoryChangeType.ReturnIn); stock.QtyBefore.Should().Be(5); stock.QtyAfter.Should().Be(7);
        stock.QtyChange.Should().Be(2); stock.ReservedChange.Should().Be(0); stock.RefType.Should().Be("OrderReturn"); stock.RefId.Should().Be(c.Request.Id);
        stock.CreatedAt.Should().Be(c.Buyer.Now); c.Buyer.History.Last().ChangedByRole.Should().Be(OrderRole.Seller);
        (await c.Received.Handle(new(c.Request.Id), default)).Error.Should().Be(ReturnErrors.InvalidStatus);
        c.WalletLedger.Should().ContainSingle(); c.StockLedger.Should().ContainSingle();
    }
    [Fact]
    public async Task MissingWarehouseStockOrApproval_FailsBeforeMutation()
    {
        var c = await SellerReturnTestContext.Create();
        (await c.Received.Handle(new(c.Request.Id), default)).Error.Should().Be(ReturnErrors.InvalidStatus);
        c.Addresses.Setup(a => a.GetReturnAddressAsync(c.Shop.Id, It.IsAny<CancellationToken>())).ReturnsAsync((ShopAddress?)null);
        (await c.Approve.Handle(new(c.Request.Id, new(null)), default)).Error.Should().Be(ReturnErrors.ReturnAddressRequired);
        c.Request.Status.Should().Be(ReturnStatus.Pending);
        c = await SellerReturnTestContext.Create(true);
        c.Inventory.Setup(i => i.GetByIdsForUpdateAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        (await c.Received.Handle(new(c.Request.Id), default)).Error.Should().Be(ReturnErrors.InventoryNotFound);
        c.Wallet.Balance.Should().Be(1000); c.Request.Status.Should().Be(ReturnStatus.SellerApproved);
    }
    [Fact]
    public void SellerValidators_CoverEveryCommandAndQuery()
    {
        new ApproveReturnCommandValidator().Validate(new ApproveReturnCommand(Guid.NewGuid(), new(null))).IsValid.Should().BeTrue();
        new ApproveReturnCommandValidator().Validate(new ApproveReturnCommand(Guid.Empty, new(new string('x', 2001)))).Errors.Should().HaveCount(2);
        new RejectReturnCommandValidator().Validate(new RejectReturnCommand(Guid.NewGuid(), new("Reason", ["https://example.com/evidence"]))).IsValid.Should().BeTrue();
        new RejectReturnCommandValidator().Validate(new RejectReturnCommand(Guid.Empty, new(" ", []))).IsValid.Should().BeFalse();
        new RejectReturnCommandValidator().Validate(new RejectReturnCommand(Guid.NewGuid(), new("Reason", ["bad-url"]))).IsValid.Should().BeFalse();
        new ConfirmReturnReceivedCommandValidator().Validate(new ConfirmReturnReceivedCommand(Guid.Empty)).IsValid.Should().BeFalse();
        new GetSellerReturnsQueryValidator().Validate(new GetSellerReturnsQuery("SellerApproved", 1, 20)).IsValid.Should().BeTrue();
        new GetSellerReturnsQueryValidator().Validate(new GetSellerReturnsQuery("8", 0, 101)).Errors.Should().HaveCount(3);
    }
}
