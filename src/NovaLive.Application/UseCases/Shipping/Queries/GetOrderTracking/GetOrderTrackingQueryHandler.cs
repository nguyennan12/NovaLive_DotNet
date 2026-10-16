using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shipping;

namespace NovaLive.Application.UseCases.Shipping.Queries.GetOrderTracking;

public sealed class GetOrderTrackingQueryHandler(
    IParentOrderRepository parents, ISubOrderRepository subs, IShippingOrderRepository shipments,
    ICurrentUser user, ICurrentShop currentShop, OrderShopAccess shopAccess, IShippingProviderResolver providers)
    : IQueryHandler<GetOrderTrackingQuery, IReadOnlyList<TrackingResponse>>
{
    public async Task<Result<IReadOnlyList<TrackingResponse>>> Handle(GetOrderTrackingQuery query, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } actor || actor == Guid.Empty)
            return ShippingErrors.Unauthorized;
        List<ShippingOrder> orders;
        if (query.IsSeller)
        {
            var access = await shopAccess.GetAsync(ct);
            if (access.IsFailure) return ShippingErrors.SubOrderNotFound;
            var sub = await subs.FindForShopAsync(query.OrderId, access.Value, ct);
            if (sub is null || sub.ShopId != currentShop.ShopId) return ShippingErrors.SubOrderNotFound;
            var shipment = await shipments.FindBySubOrderIdAsync(sub.Id, ct);
            if (shipment is null) return ShippingErrors.ShipmentNotFound;
            orders = [shipment];
        }
        else
        {
            var parent = await parents.FindAsync(query.OrderId, actor, ct);
            if (parent is null || parent.BuyerId != actor) return ShippingErrors.OrderNotFound;
            var children = await subs.GetByParentIdsAsync([parent.Id], false, ct);
            orders = await shipments.GetBySubOrderIdsAsync(children.Select(s => s.Id), ct);
        }

        var responses = new List<TrackingResponse>();
        foreach (var shipment in orders)
        {
            var response = new TrackingResponse(shipment.SubOrderId, shipment.Provider.ToString(),
                shipment.TrackingCode ?? "", shipment.Status.ToString(), [], true);
            var resolved = providers.Resolve(shipment.Provider.ToString());
            if (resolved.IsSuccess && !string.IsNullOrWhiteSpace(shipment.TrackingCode))
            {
                try
                {
                    var result = await resolved.Value!.GetTrackingAsync(shipment.TrackingCode, ct);
                    if (result.IsSuccess)
                        response = response with { Status = result.Value!.Status, Events = result.Value.Events, IsStale = false };
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception) { /* Keep the persisted status during provider outages. */ }
            }
            responses.Add(response);
        }
        return responses;
    }
}
