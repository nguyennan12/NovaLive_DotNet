using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Orders;
using NovaLive.Application.Abstractions.Services;
namespace NovaLive.Application.UseCases.Orders;
public sealed class OrderReservationReleaser(IOrderItemRepository items, IInventoryRepository inventories, IFlashSaleStockService flashSales)
{
    public async Task<Result> ReleaseAsync(ParentOrder parent, IReadOnlyList<SubOrder> children, Guid actor, CancellationToken ct)
    {
        var subMap = children.ToDictionary(s => s.Id);
        var lines = await items.GetBySubOrderIdsAsync(subMap.Keys, ct);
        var quantities = lines.GroupBy(i => i.SkuId).ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
        var stock = (await inventories.GetByIdsForUpdateAsync(quantities.Keys.Order(), ct)).ToDictionary(i => i.SkuId);
        foreach (var (sku, qty) in quantities)
        {
            var line = lines.First(i => i.SkuId == sku);
            if (!stock.TryGetValue(sku, out var inv) || inv.ShopId != subMap[line.SubOrderId].ShopId || inv.ReservedQty < qty)
                return Result.Failure(InventoryReservationErrors.InvalidRelease(sku));
        }
        foreach (var (sku, qty) in quantities.OrderBy(q => q.Key))
        {
            var inv = stock[sku]; var before = inv.ReservedQty;
            var released = inv.TryRelease(qty);
            if (released.IsFailure) return released;
            await inventories.AddHistoryAsync(new InventoryHistory(inv.Id, sku, InventoryChangeType.ReserveRelease,
                inv.QtyOnHand, 0, before, -qty, inv.QtyOnHand, refType: "Order", refId: parent.Id, createdBy: actor), ct);
        }
        return await flashSales.ReleaseReservedAsync(subMap.Keys, ct);
    }
}
