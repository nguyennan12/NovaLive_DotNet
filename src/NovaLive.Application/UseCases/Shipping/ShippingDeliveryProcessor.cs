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
        bool rejectInvalidTransition = false, bool isSellerConfirmedDelivery = false)
    {
        var normalizedTrackingCode = trackingCode.Trim();
        var shipment = await shipments.GetByTrackingForUpdateAsync(provider, normalizedTrackingCode, ct);
        if (shipment is null) return ShippingErrors.OrderNotFound;
        var sub = await subs.GetForUpdateAsync(shipment.SubOrderId, ct);
        if (sub is null) return ShippingErrors.SubOrderNotFound;

        var now = clock.UtcNow;
        var shipmentUpdated = shipment.UpdateStatusFromWebhook(status, rawPayload, now);
        var isVerifiedDeliveredWebhook = status == ShippingStatus.Delivered && isVerifiedWebhook;
        var isSellerConfirmedDelivered = status == ShippingStatus.Delivered && isSellerConfirmedDelivery;
        if (isVerifiedDeliveredWebhook && !shipmentUpdated && shipment.Status != ShippingStatus.Delivered)
        {
            logger.LogWarning(
                "Ignoring stale delivered webhook for {Provider}/{TrackingCode}; shipment is already {ShipmentStatus}",
                provider, normalizedTrackingCode, shipment.Status);
            return Result.Success();
        }
        if (!shipmentUpdated && !isVerifiedDeliveredWebhook && !isSellerConfirmedDelivered)
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

            // A COD settlement signal must come from an authenticated carrier webhook or
            // an explicit seller confirmation. Ordinary polling state updates are not enough.
            var hasSettlementEligibleDeliverySignal = (isVerifiedDeliveredWebhook || isSellerConfirmedDelivered) && !newlyDelivered
                && await outbox.HasCodSettlementEligibleShipmentDeliveredEventAsync(shipment.Id, ct);
            // An explicit seller reconciliation may be repeated to repair downstream settlement
            // (wallet/payment/inventory) after an older deployment or transient consumer failure.
            // Those consumers use immutable/idempotent ledgers, while the order timeline below
            // remains single-write through newlyDelivered.
            if (newlyDelivered || isSellerConfirmedDelivered
                || (isVerifiedDeliveredWebhook && !hasSettlementEligibleDeliverySignal))
                await outbox.AddAsync(new OutboxMessage(nameof(ShipmentDeliveredEvent), JsonSerializer.Serialize(
                    new ShipmentDeliveredEvent(shipment.Id, sub.Id, sub.ParentOrderId, sub.ShopId,
                        shipment.Provider.ToString(), shipment.TrackingCode!, sub.DeliveredAt!.Value,
                        isVerifiedWebhook, isSellerConfirmedDelivery), Json), now), ct);
        }
        else if (status is ShippingStatus.Failed or ShippingStatus.Returned or ShippingStatus.Cancelled)
            logger.LogWarning("Carrier reported {Status} for {Provider}/{TrackingCode}; SubOrder {SubOrderId} remains {SubOrderStatus}",
                status, provider, normalizedTrackingCode, sub.Id, sub.Status);

        return Result.Success();
    }
}
