using System.Text.Json;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Shipping.Commands.Webhook;

public sealed class HandleShippingWebhookCommandHandler(
    IShippingOrderRepository shipments,
    ISubOrderRepository subs,
    IOrderStatusHistoryRepository histories,
    IOutboxRepository outbox,
    IDateTimeProvider clock,
    ILogger<HandleShippingWebhookCommandHandler> logger) : ICommandHandler<HandleShippingWebhookCommand>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<Result> Handle(HandleShippingWebhookCommand command, CancellationToken ct)
    {
        // Serialize webhook and polling transactions on the same shipment before reading its state.
        var shipment = await shipments.GetByTrackingForUpdateAsync(command.Provider, command.TrackingCode.Trim(), ct);
        if (shipment is null) return ShippingErrors.OrderNotFound;
        var sub = await subs.GetForUpdateAsync(shipment.SubOrderId, ct);
        if (sub is null) return ShippingErrors.SubOrderNotFound;

        var now = clock.UtcNow;
        var shipmentUpdated = shipment.UpdateStatusFromWebhook(command.Status, command.RawPayload, now);
        var isVerifiedDeliveredWebhook = command.Status == ShippingStatus.Delivered
            && command.IsVerifiedWebhook;
        if (isVerifiedDeliveredWebhook && !shipmentUpdated && shipment.Status != ShippingStatus.Delivered)
        {
            logger.LogWarning(
                "Ignoring stale delivered webhook for {Provider}/{TrackingCode}; shipment is already {ShipmentStatus}",
                command.Provider, command.TrackingCode, shipment.Status);
            return Result.Success();
        }
        if (!shipmentUpdated && !isVerifiedDeliveredWebhook)
            return Result.Success();

        if (command.Status == ShippingStatus.Delivered)
        {
            var newlyDelivered = sub.MarkAsDelivered(now);
            if (!newlyDelivered && sub.DeliveredAt is null)
                return OrderStateErrors.InvalidStatusTransition;

            if (newlyDelivered)
                await histories.AddAsync(OrderStatusHistory.Create(sub.Id, SubOrderStatus.Shipping,
                    SubOrderStatus.Delivered, null, OrderRole.System, "Shipment delivered", now), ct);

            // A verified callback may arrive after polling has already marked delivery.
            // Emit again so COD settlement can consume the authenticated signal; ledger
            // idempotency in the consumer prevents duplicate wallet credits.
            if (newlyDelivered || isVerifiedDeliveredWebhook)
                await outbox.AddAsync(new OutboxMessage(nameof(ShipmentDeliveredEvent), JsonSerializer.Serialize(
                    new ShipmentDeliveredEvent(shipment.Id, sub.Id, sub.ParentOrderId, sub.ShopId,
                        shipment.Provider.ToString(), shipment.TrackingCode!, sub.DeliveredAt!.Value,
                        command.IsVerifiedWebhook), Json), now), ct);
        }
        else if (command.Status is ShippingStatus.Failed or ShippingStatus.Returned or ShippingStatus.Cancelled)
            logger.LogWarning("Carrier reported {Status} for {Provider}/{TrackingCode}; SubOrder {SubOrderId} remains {SubOrderStatus}",
                command.Status, command.Provider, command.TrackingCode, sub.Id, sub.Status);

        return Result.Success();
    }
}
