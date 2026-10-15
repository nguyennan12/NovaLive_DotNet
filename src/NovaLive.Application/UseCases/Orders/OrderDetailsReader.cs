using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
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
