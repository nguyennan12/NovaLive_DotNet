using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Shops;
namespace NovaLive.Domain.Tests.Returns;
public sealed class SellerReturnDomainTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-17T00:00:00Z");
    private static OrderReturn Request() => OrderReturn.Create(Guid.NewGuid(), Guid.NewGuid(), ReturnReason.Defective, ["evidence"], 100, Now);
    [Fact]
    public void ApproveAndReceive_OnlyFollowAllowedTransitions()
    {
        var request = Request(); request.ConfirmReceived(Guid.NewGuid(), Now).Error.Should().Be(ReturnStateErrors.InvalidStatus);
        request.Approve("approved", Now).IsSuccess.Should().BeTrue(); request.Reject("reject", Now).IsFailure.Should().BeTrue();
        request.ConfirmReceived(Guid.NewGuid(), Now).IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ReturnStatus.Completed); request.ResolvedAt.Should().Be(Now);
        request.ConfirmReceived(Guid.NewGuid(), Now.AddDays(1)).IsFailure.Should().BeTrue(); request.ResolvedAt.Should().Be(Now);
    }
    [Fact]
    public void Reject_RequiresPendingAndDoesNotComplete()
    {
        var request = Request(); request.Reject(" ", Now).IsFailure.Should().BeTrue();
        request.Reject("reason/evidence", Now).IsSuccess.Should().BeTrue();
        request.Approve("approve", Now).IsFailure.Should().BeTrue(); request.Reject("again", Now).IsFailure.Should().BeTrue();
        request.Status.Should().Be(ReturnStatus.SellerRejected); request.ResolvedAt.Should().BeNull();
    }
    [Fact]
    public void SellerDeadline_IncludesThirdDayAndRejectsLaterWithoutMutation()
    {
        Request().Approve("ok", Now.AddDays(3)).IsSuccess.Should().BeTrue();
        var late = Request(); late.Reject("reason", Now.AddDays(3).AddTicks(1)).Error.Should().Be(ReturnStateErrors.ResponseExpired);
        late.Status.Should().Be(ReturnStatus.Pending); late.SellerResponse.Should().BeNull();
    }
    [Fact]
    public void RefundDebitAndStockReceipt_RejectInvalidAmountsWithoutMutation()
    {
        var wallet = new ShopWallet(Guid.NewGuid()); wallet.Credit(100, Now);
        wallet.DebitRefund(101, Now).IsFailure.Should().BeTrue(); wallet.DebitRefund(-1, Now).IsFailure.Should().BeTrue();
        wallet.Balance.Should().Be(100); wallet.DebitRefund(100, Now).IsSuccess.Should().BeTrue(); wallet.Balance.Should().Be(0);
        var stock = new NovaLive.Domain.Inventory.Inventory(Guid.NewGuid(), Guid.NewGuid(), 2);
        stock.ReceiveReturn(0, Now).IsFailure.Should().BeTrue(); stock.ReceiveReturn(int.MaxValue, Now).IsFailure.Should().BeTrue();
        stock.QtyOnHand.Should().Be(2); stock.ReceiveReturn(3, Now).IsSuccess.Should().BeTrue();
        stock.QtyOnHand.Should().Be(5); stock.LastUpdated.Should().Be(Now);
    }
    [Fact]
    public void SubOrder_OnlyReturnRequestedCanBecomeReturned()
    {
        var sub = SubOrder.Create(Guid.NewGuid(), "SUB", Guid.NewGuid(), new(Guid.NewGuid(), 100, 10, 0, 0, 0, 100, 110, []), true, Now);
        sub.MarkAsReturned(Now).IsFailure.Should().BeTrue(); sub.MarkAsShipping(Now); sub.MarkAsDelivered(Now); sub.RequestReturn(Now);
        sub.MarkAsReturned(Now).IsSuccess.Should().BeTrue(); sub.MarkAsReturned(Now.AddDays(1)).IsFailure.Should().BeTrue();
        sub.Status.Should().Be(SubOrderStatus.Returned); sub.DeliveredAt.Should().Be(Now);
    }
}
