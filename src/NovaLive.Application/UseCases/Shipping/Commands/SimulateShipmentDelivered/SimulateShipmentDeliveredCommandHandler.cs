using System.Text.Json;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;

namespace NovaLive.Application.UseCases.Shipping.Commands.SimulateShipmentDelivered;

public sealed class SimulateShipmentDeliveredCommandHandler(
    ISubOrderRepository subs,
    IShippingOrderRepository shipments,
    ICurrentUser user,
    OrderShopAccess shopAccess,
    ShippingDeliveryProcessor delivery)
    : ICommandHandler<SimulateShipmentDeliveredCommand>
{
    public async Task<Result> Handle(SimulateShipmentDeliveredCommand command, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } actor || actor == Guid.Empty)
            return ShippingErrors.Unauthorized;

        var access = await shopAccess.GetAsync(ct);
        if (access.IsFailure) return ShippingErrors.SubOrderNotFound;
        var shopId = access.Value;
        var sub = await subs.FindForShopAsync(command.SubOrderId, shopId, ct);
        if (sub is null || sub.ShopId != shopId) return ShippingErrors.SubOrderNotFound;

        var shipment = await shipments.FindBySubOrderIdAsync(sub.Id, ct);
        if (shipment is null || string.IsNullOrWhiteSpace(shipment.TrackingCode))
            return ShippingErrors.ShipmentNotFound;
        if (shipment.Provider != ShippingProvider.GHN)
            return ShippingErrors.UnsupportedProvider;
        if (sub.Status == SubOrderStatus.Delivered && shipment.Status == ShippingStatus.Delivered)
            return Result.Success();
        if (sub.Status != SubOrderStatus.Shipping
            || shipment.Status is ShippingStatus.Delivered or ShippingStatus.Failed
                or ShippingStatus.Returned or ShippingStatus.Cancelled)
            return ShippingErrors.InvalidDeliverySimulationStatus;

        var payload = JsonSerializer.Serialize(new
        {
            source = "seller-demo",
            actorId = actor,
            subOrderId = sub.Id,
            status = "delivered"
        });

        // False is intentional: the demo action must never be treated as authenticated
        // carrier evidence and therefore cannot trigger automatic COD settlement.
        return await delivery.ProcessAsync(ShippingProvider.GHN, shipment.TrackingCode,
            ShippingStatus.Delivered, payload, isVerifiedWebhook: false, ct,
            rejectInvalidTransition: true);
    }
}
