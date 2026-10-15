using System.Text.Json;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
namespace NovaLive.Application.UseCases.Orders;
public static class OrderMappings
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public static ParentOrderListItemResponse ToListItem(this ParentOrder p) =>
        new(p.Id, p.OrderCode, p.GrandTotal, p.PaymentStatus.ToString(), p.CreatedAt);
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
