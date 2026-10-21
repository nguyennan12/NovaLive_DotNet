using FluentAssertions;
using Moq;
using NovaLive.Application.UseCases.Inventory;
using NovaLive.Application.UseCases.Inventory.Commands.ConfirmInventorySale;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;

namespace NovaLive.Application.Tests.Inventory;

public sealed class ConfirmInventorySaleCommandHandlerTests
{
    [Fact]
    public void Validator_RequiresPaymentId()
    {
        var validator = new ConfirmInventorySaleCommandValidator();
        validator.Validate(new ConfirmInventorySaleCommand(Guid.Empty)).IsValid.Should().BeFalse();
        validator.Validate(new ConfirmInventorySaleCommand(Guid.NewGuid())).IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task MultipleShopsAndSkus_AggregateLinesWriteLedgerInSkuOrder_AndReplayWithoutRedisDoesNothing()
    {
        var c = await InventorySaleTestContext.CreateAsync();
        var line = c.Checkout.ItemList[0];
        c.Checkout.ItemList.Add(OrderItem.Create(line.SubOrderId, "{}", 100,
            new OrderItemCalculationResult(line.SkuId, Guid.NewGuid(), 1, 100, 100, 0, 0, 0, 100)));
        c.Checkout.Draft.Stocks[line.SkuId].Reserve(1);
        var command = new ConfirmInventorySaleCommand(c.Payment.Id);
        (await c.Handler.Handle(command, default)).IsSuccess.Should().BeTrue();
        c.ChangedSkus.Should().Equal(c.ChangedSkus.Order());
        var ledger = c.Checkout.Ledger.Where(h => h.ChangeType == InventoryChangeType.SaleConfirmed).ToList();
        ledger.Should().HaveCount(2).And.OnlyContain(h => h.RefType == "Order" && h.CreatedBy == null
            && h.QtyChange == h.ReservedChange && h.QtyAfter == h.QtyBefore + h.QtyChange
            && h.CreatedAt == c.Checkout.Draft.Now);
        ledger.Single(h => h.SkuId == line.SkuId).QtyChange.Should().Be(-3);
        c.Checkout.Draft.Stocks.Values.Should().OnlyContain(s => s.ReservedQty == 0);

        (await c.Handler.Handle(command, default)).IsSuccess.Should().BeTrue();
        c.Checkout.Ledger.Where(h => h.ChangeType == InventoryChangeType.SaleConfirmed).Should().HaveCount(2);
        c.ChangedSkus.Should().HaveCount(2);
        c.Payment.Status.Should().Be(PaymentStatus.Success);
        c.Checkout.SubList.Should().OnlyContain(s => s.Status == SubOrderStatus.PendingPayment);
    }

    [Fact]
    public async Task LatePayment_SkipsCancelledSubOrderAndItsReleasedStock()
    {
        var c = await InventorySaleTestContext.CreateAsync();
        var cancelled = c.Checkout.SubList[0];
        cancelled.Cancel(Guid.NewGuid(), OrderRole.Buyer, "timeout", c.Checkout.Draft.Now);
        var sku = c.Checkout.ItemList.Single(i => i.SubOrderId == cancelled.Id).SkuId;
        c.Checkout.Draft.Stocks[sku].TryRelease(2, c.Checkout.Draft.Now);
        (await c.Handler.Handle(new(c.Payment.Id), default)).IsSuccess.Should().BeTrue();
        c.ChangedSkus.Should().NotContain(sku);
        c.Checkout.Ledger.Should().NotContain(h => h.RefId == cancelled.Id && h.ChangeType == InventoryChangeType.SaleConfirmed);
        c.FlashSales.Verify(f => f.ConfirmSoldAsync(It.Is<IEnumerable<Guid>>(ids => !ids.Contains(cancelled.Id)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReservationMismatch_ReturnsExplicitFailure()
    {
        var c = await InventorySaleTestContext.CreateAsync();
        c.FailSku = c.Checkout.ItemList.MinBy(i => i.SkuId)!.SkuId;
        (await c.Handler.Handle(new(c.Payment.Id), default)).Error.Should().Be(InventoryErrors.ReservationMismatch);
        c.Checkout.Ledger.Should().NotContain(h => h.ChangeType == InventoryChangeType.SaleConfirmed);
        c.FlashSales.Verify(f => f.ConfirmSoldAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PendingPayment_IsRejectedBeforeAnyStockChange()
    {
        var c = await InventorySaleTestContext.CreateAsync(paid: false);
        (await c.Handler.Handle(new(c.Payment.Id), default)).Error.Should().Be(InventoryErrors.PaymentNotSuccessful);
        c.ChangedSkus.Should().BeEmpty();
    }
}
