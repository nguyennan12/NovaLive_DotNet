using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Pricing;
namespace NovaLive.Domain.Tests.Orders;
public sealed class CheckoutDomainTests
{
    [Fact]
    public void ReserveAndRelease_EnforceBoundsWithoutChangingOnHand()
    {
        var inventory = new NovaLive.Domain.Inventory.Inventory(Guid.NewGuid(), Guid.NewGuid(), 5);
        inventory.TryReserve(4).IsSuccess.Should().BeTrue();
        inventory.TryReserve(2).IsFailure.Should().BeTrue(); inventory.ReservedQty.Should().Be(4);
        inventory.TryRelease(5).IsFailure.Should().BeTrue(); inventory.TryRelease(4).IsSuccess.Should().BeTrue();
        inventory.ReservedQty.Should().Be(0); inventory.QtyOnHand.Should().Be(5);
    }
    [Fact]
    public void CreatesOrderSnapshotsAndCodConfirmationAndOnlineExpiry()
    {
        var now = DateTimeOffset.UtcNow; var code = OrderCodeGenerator.Generate(now);
        code.Should().MatchRegex("^NOVA-[0-9]{8}-[A-Z0-9]{4}$");
        var parent = ParentOrder.Create(Guid.NewGuid(), code, "{}", 200, 30, 10, 220, "note", now);
        parent.CurrencyCode.Should().Be("VND"); parent.PaymentStatus.Should().Be(ParentOrderPaymentStatus.Pending);
        var line = new OrderItemCalculationResult(Guid.NewGuid(), Guid.NewGuid(), 2, 100, 200, 0, 10, 10, 190);
        var price = new ShopCalculationResult(Guid.NewGuid(), 200, 30, 0, 10, 0, 190, 220, [line]);
        var sub = SubOrder.Create(parent.Id, code + "-S1", Guid.NewGuid(), price, true, now);
        sub.Status.Should().Be(SubOrderStatus.Confirmed); sub.ConfirmedAt.Should().Be(now);
        var item = OrderItem.Create(sub.Id, "{\"skuCode\":\"A\"}", 120, line);
        item.LineTotal.Should().Be(item.UnitPrice * item.Quantity - item.DiscountAmount);
        Payment.Create(parent.Id, PaymentMethod.MoMo, 220, now).ExpiredAt.Should().Be(now.AddMinutes(15));
        Payment.Create(parent.Id, PaymentMethod.COD, 220, now).ExpiredAt.Should().BeNull();
    }
}
