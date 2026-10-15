using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
namespace NovaLive.Domain.Tests.Orders;
public sealed class OrderTransitionTests
{
    private static SubOrder Create(bool cod)
    {
        var shop = new ShopCalculationResult(Guid.NewGuid(), 100, 30, 0, 0, 0, 100, 130, []);
        return SubOrder.Create(Guid.NewGuid(), "NOVA-20261015-AB12-S1", Guid.NewGuid(), shop, cod, DateTimeOffset.UtcNow);
    }
    [Fact]
    public void BuyerCancellationAndRepeatedCancellation_EnforceTransitions()
    {
        var sub = Create(false); var actor = Guid.NewGuid(); var now = DateTimeOffset.UtcNow;
        sub.Cancel(actor, OrderRole.Buyer, "Reason", now).IsSuccess.Should().BeTrue();
        sub.Status.Should().Be(SubOrderStatus.Cancelled); sub.CancelledBy.Should().Be(actor); sub.CancelledAt.Should().Be(now);
        sub.Cancel(actor, OrderRole.Buyer, "Again", now).Error.Code.Should().Be("Orders.AlreadyCancelled");
        Create(true).Cancel(actor, OrderRole.Buyer, "Reason", now).Error.Code.Should().Be("Orders.CannotCancelConfirmedOrder");
    }
    [Fact]
    public void WarehouseAssignment_RequiresConfirmed_AndDoesNotAdvanceFulfillment()
    {
        var now = DateTimeOffset.UtcNow; var warehouse = Guid.NewGuid();
        Create(false).AssignWarehouse(warehouse, now).Error.Code.Should().Be("Orders.SubOrderNotPaid");
        var cod = Create(true); cod.AssignWarehouse(warehouse, now).IsSuccess.Should().BeTrue();
        cod.Status.Should().Be(SubOrderStatus.Confirmed); cod.WarehouseAddressId.Should().Be(warehouse);
        cod.MarkAsShipping(); cod.Cancel(Guid.NewGuid(), OrderRole.Seller, "Reason", now).IsFailure.Should().BeTrue();
    }
}
