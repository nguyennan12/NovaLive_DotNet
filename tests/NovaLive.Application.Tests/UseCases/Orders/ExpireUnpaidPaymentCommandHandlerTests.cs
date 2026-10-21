using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NovaLive.Application.Tests.Inventory;
using NovaLive.Application.UseCases.Inventory;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Application.UseCases.Orders.Commands.ExpireUnpaidPayment;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;

namespace NovaLive.Application.Tests.UseCases.Orders;

public sealed class ExpireUnpaidPaymentCommandHandlerTests
{
    private static DateTimeOffset Now(InventorySaleTestContext c) => c.Checkout.Draft.Now.AddMinutes(20);

    private static ExpireUnpaidPaymentCommandHandler Handler(InventorySaleTestContext c, bool claim = true)
    {
        c.Checkout.Draft.Clock.SetupGet(t => t.UtcNow).Returns(Now(c));
        c.Checkout.Payments.Setup(p => p.TryExpirePendingOnlineAsync(c.Payment.Id, Now(c), TimeSpan.FromMinutes(15),
                It.IsAny<CancellationToken>()))
            .Callback(() => { if (claim) c.Payment.ExpirePending(Now(c)); }).ReturnsAsync(claim);
        c.Checkout.Subs.Setup(s => s.GetForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => c.Checkout.SubList.SingleOrDefault(s => s.Id == id));
        c.Checkout.Draft.Inventories.Setup(i => i.ReleaseReservedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<int>(), Now(c), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid sku, Guid shop, int quantity, DateTimeOffset now, CancellationToken _) =>
                c.ChangeStock(sku, quantity, false, now));
        c.FlashSales.Setup(f => f.ReleaseReservedAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        return new(c.Checkout.Payments.Object, c.Checkout.Parents.Object, c.Checkout.Subs.Object, c.Checkout.Items.Object,
            c.Checkout.Draft.Inventories.Object, c.Histories.Object, c.Checkout.Histories.Object, c.Checkout.Outbox.Object,
            c.FlashSales.Object, c.Checkout.Draft.Clock.Object, Options.Create(new OrderTimeoutOptions()),
            NullLogger<ExpireUnpaidPaymentCommandHandler>.Instance);
    }

    [Theory]
    [InlineData(PaymentMethod.MoMo)]
    [InlineData(PaymentMethod.VietQR)]
    public async Task OnlinePending_ExpiresCancelsAndReleasesExactlyOnce(PaymentMethod method)
    {
        var c = await InventorySaleTestContext.CreateAsync(false);
        typeof(Domain.Payments.Payment).GetProperty("Method")!.SetValue(c.Payment, method);
        var handler = Handler(c);
        (await handler.Handle(new(c.Payment.Id), default)).IsSuccess.Should().BeTrue();
        c.Payment.Status.Should().Be(PaymentStatus.Expired);
        c.Orders.Parent.PaymentStatus.Should().Be(ParentOrderPaymentStatus.Cancelled);
        c.Checkout.SubList.Should().OnlyContain(s => s.Status == SubOrderStatus.Cancelled
            && s.CancelReason == "PaymentTimeout" && s.CancelledBy == null && s.CancelledAt == Now(c));
        c.Checkout.Draft.Stocks.Values.Should().OnlyContain(i => i.QtyOnHand == 10 && i.ReservedQty == 0);
        c.ChangedSkus.Should().Equal(c.ChangedSkus.Order());
        var ledger = c.Checkout.Ledger.Where(h => h.ChangeType == InventoryChangeType.ReserveRelease).ToList();
        ledger.Should().HaveCount(2).And.OnlyContain(h => h.QtyChange == 0 && h.ReservedChange == -2
            && h.ReservedBefore == 2 && h.QtyBefore == 10 && h.QtyAfter == 10 && h.RefType == "Order" && h.CreatedBy == null);
        c.Checkout.HistoryList.Where(h => h.ToStatus == "Cancelled").Should().HaveCount(2)
            .And.OnlyContain(h => h.ChangedByRole == OrderRole.System && h.ChangedBy == null && h.Note!.Contains("PaymentTimeout"));
        c.Checkout.Messages.Should().ContainSingle(m => m.EventType == "OrderCancelledEvent" && m.Payload.Contains("PaymentTimeout"));
        (await handler.Handle(new(c.Payment.Id), default)).IsSuccess.Should().BeTrue();
        c.ChangedSkus.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(PaymentMethod.COD, PaymentStatus.Pending)]
    [InlineData(PaymentMethod.MoMo, PaymentStatus.Success)]
    [InlineData(PaymentMethod.VietQR, PaymentStatus.Failed)]
    [InlineData(PaymentMethod.MoMo, PaymentStatus.Expired)]
    [InlineData(PaymentMethod.MoMo, PaymentStatus.Refunded)]
    public async Task CodAndNonPending_NeverClaimOrRelease(PaymentMethod method, PaymentStatus status)
    {
        var c = await InventorySaleTestContext.CreateAsync(false);
        typeof(Domain.Payments.Payment).GetProperty("Method")!.SetValue(c.Payment, method);
        typeof(Domain.Payments.Payment).GetProperty("Status")!.SetValue(c.Payment, status);
        (await Handler(c).Handle(new(c.Payment.Id), default)).IsSuccess.Should().BeTrue();
        c.Checkout.Payments.Verify(p => p.TryExpirePendingOnlineAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(),
            It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        c.ChangedSkus.Should().BeEmpty();
        c.Orders.Parent.PaymentStatus.Should().Be(ParentOrderPaymentStatus.Pending);
    }

    [Fact]
    public async Task LostClaimRace_IsNoOp()
    {
        var c = await InventorySaleTestContext.CreateAsync(false);
        (await Handler(c, claim: false).Handle(new(c.Payment.Id), default)).IsSuccess.Should().BeTrue();
        c.ChangedSkus.Should().BeEmpty();
        c.Orders.Parent.PaymentStatus.Should().Be(ParentOrderPaymentStatus.Pending);
        c.Checkout.SubList.Should().OnlyContain(s => s.Status == SubOrderStatus.PendingPayment);
        c.Checkout.HistoryList.Should().NotContain(h => h.ToStatus == "Cancelled");
    }

    [Theory]
    [InlineData(15, true)]
    [InlineData(14, false)]
    public async Task NullExpiry_UsesCreatedAtFallback(int minutes, bool shouldClaim)
    {
        var c = await InventorySaleTestContext.CreateAsync(false);
        typeof(Domain.Payments.Payment).GetProperty("ExpiredAt")!.SetValue(c.Payment, null);
        var handler = Handler(c, claim: false);
        c.Checkout.Draft.Clock.SetupGet(t => t.UtcNow).Returns(c.Checkout.Draft.Now.AddMinutes(minutes));
        (await handler.Handle(new(c.Payment.Id), default)).IsSuccess.Should().BeTrue();
        c.Checkout.Payments.Verify(p => p.TryExpirePendingOnlineAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(),
            It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), shouldClaim ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task NonPendingSubOrder_IsSkipped_AndExistingReleaseLedgerIsNotDuplicated()
    {
        var c = await InventorySaleTestContext.CreateAsync(false);
        var confirmed = c.Checkout.SubList[0]; confirmed.ConfirmPayment(c.Checkout.Draft.Now);
        var pending = c.Checkout.SubList[1];
        var line = c.Checkout.ItemList.Single(i => i.SubOrderId == pending.Id);
        var stock = c.Checkout.Draft.Stocks[line.SkuId]; stock.TryRelease(line.Quantity, Now(c));
        c.Checkout.Ledger.Add(new InventoryHistory(stock.Id, line.SkuId, InventoryChangeType.ReserveRelease,
            10, 0, 2, -2, 10, refType: "Order", refId: pending.Id));
        (await Handler(c).Handle(new(c.Payment.Id), default)).IsSuccess.Should().BeTrue();
        confirmed.Status.Should().Be(SubOrderStatus.Confirmed);
        c.ChangedSkus.Should().BeEmpty();
        c.Checkout.Ledger.Count(h => h.ChangeType == InventoryChangeType.ReserveRelease).Should().Be(1);
    }

    [Fact]
    public async Task ReservationMismatch_PropagatesFailureForTransactionRollback()
    {
        var c = await InventorySaleTestContext.CreateAsync(false);
        c.FailSku = c.Checkout.ItemList.MinBy(i => i.SkuId)!.SkuId;
        (await Handler(c).Handle(new(c.Payment.Id), default)).Error.Should().Be(InventoryErrors.ReservationMismatch);
        c.Checkout.Messages.Should().NotContain(m => m.EventType == "OrderCancelledEvent");
    }

    [Fact]
    public void Validator_RequiresPaymentId()
    {
        var validator = new ExpireUnpaidPaymentCommandValidator();
        validator.Validate(new ExpireUnpaidPaymentCommand(Guid.Empty)).IsValid.Should().BeFalse();
        validator.Validate(new ExpireUnpaidPaymentCommand(Guid.NewGuid())).IsValid.Should().BeTrue();
    }
}
