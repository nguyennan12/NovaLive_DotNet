using FluentAssertions;
using NovaLive.Application.UseCases.Orders.Commands.ConfirmSubOrder;
using NovaLive.Domain.Common;
namespace NovaLive.Application.Tests.UseCases.Orders;
public sealed class ConfirmSubOrderTests
{
    [Fact]
    public async Task AssignOwnedWarehouse_PreservesConfirmedAndRecordsHistory()
    {
        var c = await OrderOperationsTestContext.Create(cod: true); var sub = c.Checkout.SubList.Single();
        var warehouse = c.Checkout.Draft.WarehouseList.Single();
        (await c.Confirm.Handle(new(sub.Id, new(warehouse.Id)), default)).IsSuccess.Should().BeTrue();
        sub.WarehouseAddressId.Should().Be(warehouse.Id); sub.Status.Should().Be(SubOrderStatus.Confirmed);
        c.Checkout.HistoryList.Should().Contain(h => h.FromStatus == "Confirmed" && h.ToStatus == "Confirmed" && h.ChangedByRole == OrderRole.Seller);
    }
    [Fact]
    public async Task PendingPaymentAndForeignWarehouseOrSubOrder_AreRejected()
    {
        var c = await OrderOperationsTestContext.Create(shops: 2);
        var own = c.Checkout.SubList.Single(s => s.ShopId == c.HeaderShopId);
        var foreign = c.Checkout.SubList.Single(s => s.ShopId != c.HeaderShopId);
        var warehouse = c.Checkout.Draft.WarehouseList.Single(w => w.ShopId == c.HeaderShopId);
        var foreignWarehouse = c.Checkout.Draft.WarehouseList.Single(w => w.ShopId != c.HeaderShopId);
        (await c.Confirm.Handle(new(own.Id, new(warehouse.Id)), default)).Error.Code.Should().Be("Orders.SubOrderNotPaid");
        (await c.Confirm.Handle(new(own.Id, new(foreignWarehouse.Id)), default)).Error.Code.Should().Be("Orders.WarehouseNotFound");
        (await c.Confirm.Handle(new(foreign.Id, new(warehouse.Id)), default)).Error.Code.Should().Be("Orders.SubOrderNotFound");
        c.Checkout.HistoryList.Should().HaveCount(2);
    }
    [Fact]
    public void Validator_RequiresSubOrderAndWarehouse()
    {
        new ConfirmSubOrderCommandValidator().Validate(new ConfirmSubOrderCommand(Guid.Empty, new(Guid.Empty))).Errors.Should().HaveCount(2);
    }
}
