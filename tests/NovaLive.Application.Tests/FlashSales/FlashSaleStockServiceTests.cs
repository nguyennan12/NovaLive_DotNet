using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.FlashSales;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;

namespace NovaLive.Application.Tests.FlashSales;

public sealed class FlashSaleStockServiceTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Retry_TransitionsOnlyOnce_UsingPersistedOrderLine(bool confirm)
    {
        var orders = new Mock<IOrderItemRepository>(); var flash = new Mock<IFlashSaleRepository>();
        var line = CreateLine();
        orders.Setup(o => o.GetBySubOrderIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([line]);
        var target = confirm ? FlashSaleReservationState.Sold : FlashSaleReservationState.Released;
        orders.SetupSequence(o => o.TryTransitionFlashSaleAsync(line.Id, target, It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
        orders.Setup(o => o.GetFlashSaleStateAsync(line.Id, It.IsAny<CancellationToken>())).ReturnsAsync(target);
        flash.Setup(f => f.ConfirmSoldAsync(line.FlashSaleItemId!.Value, line.Quantity, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        flash.Setup(f => f.ReleaseReservedAsync(line.FlashSaleItemId!.Value, line.Quantity, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = new FlashSaleStockService(orders.Object, flash.Object);
        for (var i = 0; i < 2; i++)
            (await (confirm ? service.ConfirmSoldAsync([line.SubOrderId]) : service.ReleaseReservedAsync([line.SubOrderId])))
                .IsSuccess.Should().BeTrue();
        flash.Invocations.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InconsistentCounter_ReturnsFailureSoCallerRollsBackMarker(bool confirm)
    {
        var orders = new Mock<IOrderItemRepository>(); var flash = new Mock<IFlashSaleRepository>(); var line = CreateLine();
        orders.Setup(o => o.GetBySubOrderIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([line]);
        orders.Setup(o => o.TryTransitionFlashSaleAsync(line.Id, It.IsAny<FlashSaleReservationState>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = new FlashSaleStockService(orders.Object, flash.Object);
        var result = await (confirm ? service.ConfirmSoldAsync([line.SubOrderId]) : service.ReleaseReservedAsync([line.SubOrderId]));
        result.Error.Should().Be(FlashSalesErrors.ReservationConflict);
    }

    [Fact]
    public async Task ReleaseAlreadySold_ReturnsConflict_WithoutTouchingOtherOrdersQuota()
    {
        var orders = new Mock<IOrderItemRepository>(); var flash = new Mock<IFlashSaleRepository>(); var line = CreateLine();
        orders.Setup(o => o.GetBySubOrderIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([line]);
        orders.Setup(o => o.GetFlashSaleStateAsync(line.Id, It.IsAny<CancellationToken>())).ReturnsAsync(FlashSaleReservationState.Sold);
        (await new FlashSaleStockService(orders.Object, flash.Object).ReleaseReservedAsync([line.SubOrderId]))
            .Error.Should().Be(FlashSalesErrors.ReservationConflict);
        flash.Invocations.Should().BeEmpty();
    }

    private static OrderItem CreateLine()
    {
        var calculation = PriceCalculator.Calculate(new PriceCalculationInput(
            [new ShopCalculationInput(Guid.NewGuid(), 0, [new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2, 80)], null)],
            null, null, DateTimeOffset.UtcNow));
        return OrderItem.Create(Guid.NewGuid(), "{}", 100, calculation.Value!.ShopResults.Single().Items.Single(), Guid.NewGuid());
    }
}
