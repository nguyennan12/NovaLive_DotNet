using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using System.Text.Json;

namespace NovaLive.Application.UseCases.Orders;

public sealed class OrderDetailsReader(IParentOrderRepository parents, ISubOrderRepository subs,
    IOrderItemRepository items, IOrderStatusHistoryRepository histories, IPaymentRepository payments)
{
    public async Task<Result<ParentOrderDetailResponse>> GetBuyerAsync(Guid id, Guid buyer, CancellationToken ct)
    {
        var parent = await parents.FindAsync(id, buyer, ct);
        if (parent is null || parent.BuyerId != buyer) return OrderErrors.OrderNotFound;
        var children = await subs.GetByParentIdsAsync([id], false, ct);
        var keys = children.Select(s => s.Id).ToArray();
        var lines = await items.GetBySubOrderIdsAsync(keys, ct);
        var timeline = await histories.GetBySubOrderIdsAsync(keys, ct);
        var payment = (await payments.GetByParentIdsAsync([id], ct)).SingleOrDefault();
        return new ParentOrderDetailResponse(parent.Id, parent.OrderCode, parent.Recipient(), parent.TotalItemAmount,
            parent.TotalShippingFee, parent.TotalDiscountAmount, parent.GrandTotal, parent.CurrencyCode,
            parent.PaymentStatus.ToString(), payment?.Method.ToString() ?? "Unknown", payment?.Id, payment?.ExpiredAt,
            parent.Note, parent.CreatedAt, children.Select(s => s.ToDetail(parent, payment, lines, timeline)).ToArray());
    }
    public async Task<Result<SubOrderDetailResponse>> GetSellerAsync(Guid id, Guid shop, CancellationToken ct)
    {
        var sub = await subs.FindForShopAsync(id, shop, ct);
        if (sub is null || sub.ShopId != shop) return OrderErrors.SubOrderNotFound;
        var parent = await parents.FindAsync(sub.ParentOrderId, null, ct);
        if (parent is null) return OrderErrors.SubOrderNotFound;
        var lines = await items.GetBySubOrderIdsAsync([id], ct);
        var timeline = await histories.GetBySubOrderIdsAsync([id], ct);
        var payment = (await payments.GetByParentIdsAsync([parent.Id], ct)).SingleOrDefault();
        return sub.ToDetail(parent, payment, lines, timeline);
    }
}

public static class OrderMappings
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public static ParentOrderListItemResponse ToListItem(this ParentOrder p, IReadOnlyCollection<SubOrder> children) =>
        new(p.Id, p.OrderCode, p.GrandTotal, children.AggregateBuyerStatus(), p.PaymentStatus.ToString(), p.CreatedAt);

    public static string AggregateBuyerStatus(this IReadOnlyCollection<SubOrder> children)
    {
        if (children.Count == 0) return "pending";
        if (children.All(s => s.Status == SubOrderStatus.Cancelled)) return "cancelled";
        if (children.Any(s => s.Status == SubOrderStatus.PendingPayment)) return "pending";
        if (children.Any(s => s.Status is SubOrderStatus.Confirmed or SubOrderStatus.Processing)) return "processing";
        if (children.Any(s => s.Status == SubOrderStatus.Shipping)) return "shipped";
        if (children.All(s => s.Status is SubOrderStatus.Delivered or SubOrderStatus.Completed or SubOrderStatus.Cancelled)) return "delivered";
        return "processing";
    }
    public static SubOrderListItemResponse ToListItem(this SubOrder s) =>
        new(s.Id, s.ParentOrderId, s.SubOrderCode, s.ShopId, s.SubTotal, s.Status.ToString(), s.CreatedAt);
    public static OrderRecipientResponse Recipient(this ParentOrder p) =>
        JsonSerializer.Deserialize<OrderRecipientResponse>(p.ShippingAddressJson, Json)
        ?? new("", "", null, "", null, "", null, "", "");
    public static SubOrderDetailResponse ToDetail(this SubOrder s, ParentOrder parent, Payment? payment,
        IEnumerable<OrderItem> items, IEnumerable<OrderStatusHistory> history) => new(
            s.Id, s.ParentOrderId, s.SubOrderCode, s.ShopId, s.WarehouseAddressId, s.OrderSource.ToString(), parent.Recipient(),
            s.ItemAmount, s.ShippingFee, s.ShopDiscountAmount, s.PlatformDiscountAmount, s.ShippingDiscountAmount,
            s.SubTotal, s.Status.ToString(), s.CancelReason, parent.PaymentStatus.ToString(), payment?.Method.ToString() ?? "Unknown",
            s.CreatedAt, items.Where(i => i.SubOrderId == s.Id).Select(i => new OrderItemDetailResponse(
                i.Id, i.SkuId, i.SkuSnapshotJson, i.Quantity, i.UnitPrice, i.OriginalPrice, i.DiscountAmount, i.LineTotal)).ToArray(),
            history.Where(h => h.SubOrderId == s.Id).OrderBy(h => h.ChangedAt).ThenBy(h => h.FromStatus is null ? 0 : 1)
                .ThenBy(h => h.Id).Select(h => new OrderTimelineResponse(h.FromStatus, h.ToStatus,
                    h.ChangedByRole?.ToString(), h.Note, h.ChangedAt)).ToArray());
}

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

public sealed class OrderShopAccess(ShopAccess access, ICurrentShop currentShop)
{
    public async Task<Result<Guid>> GetAsync(CancellationToken ct)
    {
        if (currentShop.ShopId is not { } id || id == Guid.Empty) return OrderErrors.ShopContextRequired;
        var owned = await access.GetSellerOwnedAsync(false, ct);
        // CurrentShop may read X-Shop-Id: never treat that client-controlled context as proof of ownership.
        if (owned.IsFailure || owned.Value!.Id != id) return OrderErrors.SubOrderNotFound;
        return id;
    }
}
