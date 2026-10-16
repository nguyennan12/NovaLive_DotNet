using System.Text.Json;
using FluentAssertions;
using Moq;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Application.UseCases.Orders.Commands.Checkout;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;
namespace NovaLive.Application.Tests.UseCases.Orders;
public sealed class CheckoutCommandTests
{
    [Theory]
    [InlineData("MoMo", "PendingPayment", 1)]
    [InlineData("VietQR", "PendingPayment", 1)]
    [InlineData("COD", "Confirmed", 2)]
    public async Task Checkout_UsesCurrentPricesAndSnapshots_RecordsAllWrites(string method, string status, int histories)
    {
        var c = new CheckoutCommandTestContext(); var selected = c.Request;
        c.Draft.AddShop(); // This additional cart item is deliberately not selected.
        var result = await c.Run(selected with { PaymentMethod = method });
        result.IsSuccess.Should().BeTrue(); c.Committed.Should().BeTrue();
        result.Value!.GrandTotal.Should().Be(230m); result.Value.SubOrders.Should().ContainSingle(s => s.Status == status);
        c.ParentList.Single().PaymentStatus.Should().Be(ParentOrderPaymentStatus.Pending);
        c.ParentList.Single().ShippingAddressJson.Should().Contain("recipientName").And.NotContain("userId");
        c.PaymentList.Single().Status.Should().Be(PaymentStatus.Pending);
        c.PaymentList.Single().ExpiredAt.Should().Be(method == "COD" ? null : c.Draft.Now.AddMinutes(15));
        c.Ledger.Should().ContainSingle(h => h.ReservedBefore == 0 && h.ReservedChange == 2 && h.QtyChange == 0 && h.RefType == "Order");
        c.Draft.Stocks[c.Draft.SkuList[0].Id].ReservedQty.Should().Be(2);
        c.HistoryList.Should().HaveCount(histories);
        c.ItemList.Single().UnitPrice.Should().Be(100m);
        using var snapshot = JsonDocument.Parse(c.ItemList.Single().SkuSnapshotJson);
        snapshot.RootElement.GetProperty("attributes").ValueKind.Should().Be(JsonValueKind.Object);
        c.Messages.Should().ContainSingle(m => m.EventType == "OrderPlacedEvent" && m.Payload.Contains("paymentMethod"));
        c.Draft.Carts.Verify(x => x.RemoveItems(It.Is<IEnumerable<NovaLive.Domain.Carts.CartItem>>(items =>
            items.Count() == 1 && items.First().Id == selected.CartItemIds[0])), Times.Once);
    }
    [Fact]
    public async Task MultipleShops_AllocatesVoucherAndUsagesExactly()
    {
        var c = new CheckoutCommandTestContext(); c.Draft.AddShop(101m, 3);
        c.Draft.AddVoucher("PRODUCT", value: 17); c.Draft.AddVoucher("SHIP", type: DiscountType.FreeShip, value: 7);
        var result = await c.Run(c.Request with { PlatformProductVoucherCode = "PRODUCT", PlatformFreeshipVoucherCode = "SHIP" });
        result.IsSuccess.Should().BeTrue(); c.SubList.Should().HaveCount(2);
        c.SubList.Sum(s => s.SubTotal).Should().Be(result.Value!.GrandTotal);
        c.ItemList.Sum(i => i.DiscountAmount).Should().Be(17);
        c.Usages.Sum(u => u.DiscountAmount).Should().Be(24);
        c.SubList.Sum(s => s.ShippingDiscountAmount).Should().Be(7);
    }
    [Fact]
    public async Task StockChangedAfterQuote_FailsWithoutCommitOrOrder()
    {
        var c = new CheckoutCommandTestContext();
        c.Draft.Inventories.Setup(x => x.GetByIdsForUpdateAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new NovaLive.Domain.Inventory.Inventory(c.Draft.SkuList[0].Id, c.Draft.ShopList[0].Id, 1)]);
        (await c.Run()).Error.Code.Should().Be("Checkout.InsufficientStock");
        c.Committed.Should().BeFalse(); c.ParentList.Should().BeEmpty(); c.Ledger.Should().BeEmpty();
    }
    [Theory]
    [InlineData("foreignCart", "Checkout.CartItemsNotFound")]
    [InlineData("suspended", "Checkout.ShopUnavailable")]
    [InlineData("voucher", "Checkout.InvalidVoucher")]
    [InlineData("provider", "Orders.ShippingProviderRequired")]
    [InlineData("zero", "Orders.ZeroAmountNotAllowed")]
    public async Task InvalidPreparation_DoesNotOpenTransaction(string scenario, string code)
    {
        var c = new CheckoutCommandTestContext(); var request = c.Request;
        if (scenario == "foreignCart") request = request with { CartItemIds = [Guid.NewGuid()] };
        if (scenario == "suspended") c.Draft.ShopList[0].Suspend();
        if (scenario == "voucher") request = request with { PlatformProductVoucherCode = "UNKNOWN" };
        if (scenario == "provider") request = request with { ShippingProviders = [] };
        if (scenario == "zero")
        {
            c.Draft.AddVoucher("FREE", value: 200); request = request with { PlatformProductVoucherCode = "FREE" };
            c.Draft.Shipping.Setup(s => s.CalculateFeeAsync(It.IsAny<ShippingFeeCalculationModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new ShippingFeeQuote("GHN", "2", 0)));
        }
        (await c.Run(request)).Error.Code.Should().Be(code);
        c.Transactions.Invocations.Should().BeEmpty();
    }
    [Fact]
    public async Task VoucherRace_AfterWrites_ReturnsFailureToRollbackBoundary()
    {
        var c = new CheckoutCommandTestContext(); c.Draft.AddVoucher("LIMIT");
        c.Draft.Discounts.Setup(d => d.TryConsumeAsync(It.IsAny<Guid>(), c.Draft.UserId, c.Draft.Now, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        (await c.Run(c.Request with { PlatformProductVoucherCode = "LIMIT" })).IsFailure.Should().BeTrue();
        c.Committed.Should().BeFalse(); c.Messages.Should().BeEmpty();
    }
    [Fact]
    public async Task DurableReceipt_RetryReturnsSameOrderWithoutCartOrStockWrites()
    {
        var c = new CheckoutCommandTestContext(); var first = await c.Run();
        c.Outbox.Setup(o => o.FindCheckoutReceiptAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(first.Value);
        var second = await c.Run(); second.Value.Should().BeSameAs(first.Value);
        c.ParentList.Should().ContainSingle(); c.Ledger.Should().ContainSingle();
    }
    [Fact]
    public async Task OrderCodeCollision_RetriesWithinBound()
    {
        var c = new CheckoutCommandTestContext();
        c.Parents.SetupSequence(p => p.TryAddAsync(It.IsAny<NovaLive.Domain.Orders.ParentOrder>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false).ReturnsAsync(true);
        (await c.Run()).IsSuccess.Should().BeTrue();
        c.Parents.Verify(p => p.TryAddAsync(It.IsAny<NovaLive.Domain.Orders.ParentOrder>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
    [Fact]
    public void Validator_RejectsDuplicatesMissingKeyInvalidMethodAndProviders()
    {
        var c = new CheckoutCommandTestContext(); var v = new CheckoutCommandValidator();
        v.Validate(new CheckoutCommand(c.Request, "key")).IsValid.Should().BeTrue();
        v.Validate(new CheckoutCommand(c.Request with { CartItemIds = [Guid.Empty, Guid.Empty], PaymentMethod = "Card", ShippingProviders = [] }, "")).Errors.Count.Should().BeGreaterThan(3);
    }
}
