using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shipping.Queries.GetShippingLabel;

public sealed class GetShippingLabelQueryHandler(
    ISubOrderRepository subs, IShippingOrderRepository shipments,
    OrderShopAccess shopAccess, ICurrentShop currentShop, IShippingProviderResolver providers)
    : IQueryHandler<GetShippingLabelQuery, byte[]>
{
    public async Task<Result<byte[]>> Handle(GetShippingLabelQuery query, CancellationToken ct)
    {
        var access = await shopAccess.GetAsync(ct);
        if (access.IsFailure) return ShippingErrors.SubOrderNotFound;
        var sub = await subs.FindForShopAsync(query.SubOrderId, access.Value, ct);
        if (sub is null || sub.ShopId != currentShop.ShopId) return ShippingErrors.SubOrderNotFound;
        var shipment = await shipments.FindBySubOrderIdAsync(sub.Id, ct);
        if (shipment is null || string.IsNullOrWhiteSpace(shipment.TrackingCode)) return ShippingErrors.ShipmentNotFound;
        var resolved = providers.Resolve(shipment.Provider.ToString());
        if (resolved.IsFailure) return resolved.Error;
        try
        {
            var result = await resolved.Value!.GetShippingLabelAsync(shipment.TrackingCode, ct);
            if (result.IsFailure) return ShippingErrors.ProviderError("Shipping.ProviderError", result.Error.Message);
            // A provider print page must never be sent as a mislabeled PDF.
            if (result.Value is not { Length: >= 4 } pdf || !pdf.AsSpan(0, 4).SequenceEqual("%PDF"u8))
                return ShippingErrors.ProviderError("Shipping.ProviderError", "Đối tác không trả về file PDF hợp lệ.");
            return pdf;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception)
        {
            return ShippingErrors.ProviderError("Shipping.ProviderError", "Không thể tải phiếu giao hàng từ đối tác.");
        }
    }
}
