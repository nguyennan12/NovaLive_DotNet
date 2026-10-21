using FluentAssertions;
using NovaLive.Domain.Inventory;
using Stock = NovaLive.Domain.Inventory.Inventory;

namespace NovaLive.Domain.Tests.Inventory;

public sealed class ReservedStockTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 0, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(true, 2, 8, 3)]
    [InlineData(false, 2, 10, 3)]
    [InlineData(true, 5, 5, 0)]
    [InlineData(false, 5, 10, 0)]
    public void Transition_PreservesStockInvariant(bool sale, int quantity, int onHand, int reserved)
    {
        var stock = new Stock(Guid.NewGuid(), Guid.NewGuid(), 10);
        stock.Reserve(5);
        var result = sale ? stock.TryConfirmSale(quantity, Now) : stock.TryRelease(quantity, Now);
        result.IsSuccess.Should().BeTrue();
        stock.QtyOnHand.Should().Be(onHand);
        stock.ReservedQty.Should().Be(reserved).And.BeInRange(0, stock.QtyOnHand);
        stock.LastUpdated.Should().Be(Now);
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(true, -1)]
    [InlineData(true, 6)]
    [InlineData(false, 0)]
    [InlineData(false, -1)]
    [InlineData(false, 6)]
    public void InvalidTransition_ReturnsErrorWithoutMutation(bool sale, int quantity)
    {
        var stock = new Stock(Guid.NewGuid(), Guid.NewGuid(), 10);
        stock.Reserve(5);
        var version = stock.Version;
        var result = sale ? stock.TryConfirmSale(quantity, Now) : stock.TryRelease(quantity, Now);
        result.Error.Should().Be(InventoryReservationErrors.ReservationMismatch);
        stock.QtyOnHand.Should().Be(10); stock.ReservedQty.Should().Be(5); stock.Version.Should().Be(version);
    }
}
