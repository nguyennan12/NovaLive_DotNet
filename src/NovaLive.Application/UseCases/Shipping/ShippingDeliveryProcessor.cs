using System.Text.Json;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Shipping;

/// <summary>
/// Applies carrier delivery state through one path for authenticated webhooks,
/// background reconciliation and the seller's explicit demo action.
/// </summary>
public sealed class ShippingDeliveryProcessor(
    IShippingOrderRepository shipments,
    ISubOrderRepository subs,
    IOrderStatusHistoryRepository histories,
    IOutboxRepository outbox,
    IDateTimeProvider clock,
    ILogger<ShippingDeliveryProcessor> logger)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<Result> ProcessAsync(ShippingProvider provider, string trackingCode,
        ShippingStatus status, string? rawPayload, bool isVerifiedWebhook, CancellationToken ct,
        bool rejectInvalidTransition = false)
    {
        var normalizedTrackingCode = trackingCode.Trim();
        var shipment = await shipments.GetByTrackingForUpdateAsync(provider, normalizedTrackingCode, ct);
        if (shipment is null) return ShippingErrors.OrderNotFound;
        var sub = await subs.GetForUpdateAsync(shipment.SubOrderId, ct);
        if (sub is null) return ShippingErrors.SubOrderNotFound;

        var now = clock.UtcNow;
        var shipmentUpdated = shipment.UpdateStatusFromWebhook(status, rawPayload, now);
        var isVerifiedDeliveredWebhook = status == ShippingStatus.Delivered && isVerifiedWebhook;
        if (isVerifiedDeliveredWebhook && !shipmentUpdated && shipment.Status != ShippingStatus.Delivered)
        {
            logger.LogWarning(
                "Ignoring stale delivered webhook for {Provider}/{TrackingCode}; shipment is already {ShipmentStatus}",
                provider, normalizedTrackingCode, shipment.Status);
            return Result.Success();
        }
        if (!shipmentUpdated && !isVerifiedDeliveredWebhook)
            return rejectInvalidTransition && shipment.Status != status
                ? ShippingErrors.InvalidDeliverySimulationStatus
                : Result.Success();

        if (status == ShippingStatus.Delivered)
        {
            var newlyDelivered = sub.MarkAsDelivered(now);
            if (!newlyDelivered && sub.DeliveredAt is null)
                return OrderStateErrors.InvalidStatusTransition;

            if (newlyDelivered)
                await histories.AddAsync(OrderStatusHistory.Create(sub.Id, SubOrderStatus.Shipping,
                    SubOrderStatus.Delivered, null, OrderRole.System,
                    isVerifiedWebhook ? "Shipment delivered" : "Demo delivery confirmed by seller", now), ct);

            // Only an authenticated carrier webhook is allowed to settle COD. The demo
            // action updates shipment/order state but deliberately emits an unverified event.
            var hasVerifiedDeliverySignal = isVerifiedDeliveredWebhook && !newlyDelivered
                && await outbox.HasVerifiedShipmentDeliveredEventAsync(shipment.Id, ct);
            if (newlyDelivered || (isVerifiedDeliveredWebhook && !hasVerifiedDeliverySignal))
                await outbox.AddAsync(new OutboxMessage(nameof(ShipmentDeliveredEvent), JsonSerializer.Serialize(
                    new ShipmentDeliveredEvent(shipment.Id, sub.Id, sub.ParentOrderId, sub.ShopId,
                        shipment.Provider.ToString(), shipment.TrackingCode!, sub.DeliveredAt!.Value,
                        isVerifiedWebhook), Json), now), ct);
        }
        else if (status is ShippingStatus.Failed or ShippingStatus.Returned or ShippingStatus.Cancelled)
            logger.LogWarning("Carrier reported {Status} for {Provider}/{TrackingCode}; SubOrder {SubOrderId} remains {SubOrderStatus}",
                status, provider, normalizedTrackingCode, sub.Id, sub.Status);

        return Result.Success();
    }
}
