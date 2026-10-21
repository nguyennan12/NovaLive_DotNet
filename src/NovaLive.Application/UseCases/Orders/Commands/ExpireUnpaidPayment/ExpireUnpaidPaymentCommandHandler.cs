using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Inventory;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Orders;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Orders.Commands.ExpireUnpaidPayment;

public sealed class ExpireUnpaidPaymentCommandHandler(
    IPaymentRepository payments,
    IParentOrderRepository parents,
    ISubOrderRepository subOrders,
    IOrderItemRepository items,
    IInventoryRepository inventories,
    IInventoryHistoryRepository inventoryHistories,
    IOrderStatusHistoryRepository orderHistories,
    IOutboxRepository outbox,
    IFlashSaleStockService flashSales,
    IDateTimeProvider clock,
    IOptions<OrderTimeoutOptions> options,
    ILogger<ExpireUnpaidPaymentCommandHandler> logger) : ICommandHandler<ExpireUnpaidPaymentCommand>
{
    public async Task<Result> Handle(ExpireUnpaidPaymentCommand command, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var timeout = options.Value.PaymentTimeout;
        var payment = await payments.GetByIdAsync(command.PaymentId, ct);
        if (payment is null || !payment.IsDueForExpiry(now, timeout)) return Result.Success();

        // Parent-first agrees with cancellation/VietQR lock order. The conditional payment UPDATE
        // remains the arbiter: a stale scan or a webhook that already won must cause a no-op.
        var parent = await parents.GetForUpdateAsync(payment.ParentOrderId, ct);
        if (parent is null) return OrderErrors.OrderNotFound;
        if (!await payments.TryExpirePendingOnlineAsync(payment.Id, now, timeout, ct)) return Result.Success();

        if (parent.PaymentStatus != ParentOrderPaymentStatus.Cancelled)
        {
            var cancelled = parent.CancelPayment(now);
            if (cancelled.IsFailure) return cancelled.Error;
        }

        var candidates = await subOrders.GetByParentIdsAsync([parent.Id], false, ct);
        var cancelledSubs = new List<SubOrder>();
        foreach (var candidate in candidates.Where(s => s.Status == SubOrderStatus.PendingPayment).OrderBy(s => s.Id))
        {
            var sub = await subOrders.GetForUpdateAsync(candidate.Id, ct);
            if (sub is null) return OrderErrors.SubOrderNotFound;
            if (sub.Status != SubOrderStatus.PendingPayment) continue;
            var cancelled = sub.CancelForPaymentTimeout(now);
            if (cancelled.IsFailure) return cancelled.Error;
            cancelledSubs.Add(sub);
            await orderHistories.AddAsync(OrderStatusHistory.Create(sub.Id, SubOrderStatus.PendingPayment,
                SubOrderStatus.Cancelled, null, OrderRole.System, "PaymentTimeout: online payment expired", now), ct);
        }

        var children = cancelledSubs.ToDictionary(s => s.Id);
        var lines = await items.GetBySubOrderIdsAsync(children.Keys, ct);
        await inventories.GetByIdsForUpdateAsync(lines.Select(i => i.SkuId).Distinct().Order(), ct);
        foreach (var group in lines.GroupBy(i => new { i.SkuId, i.SubOrderId })
                     .OrderBy(g => g.Key.SkuId).ThenBy(g => g.Key.SubOrderId))
        {
            var sku = group.Key.SkuId;
            var subId = group.Key.SubOrderId;
            if (await inventoryHistories.ExistsByReferenceAsync("Order", subId, sku, InventoryChangeType.ReserveRelease, ct))
                continue;
            var quantity = group.Sum(i => i.Quantity);
            var released = await inventories.ReleaseReservedAsync(sku, children[subId].ShopId, quantity, now, ct);
            if (released.IsFailure)
            {
                logger.LogError("Timeout inventory mismatch for Payment {PaymentId}, SubOrder {SubOrderId}, SKU {SkuId}, Quantity {Quantity}",
                    payment.Id, subId, sku, quantity);
                return InventoryErrors.ReservationMismatch;
            }
            var stock = released.Value!;
            await inventoryHistories.AddAsync(new InventoryHistory(stock.InventoryId, sku, InventoryChangeType.ReserveRelease,
                stock.QtyOnHand, 0, stock.ReservedQty + quantity, -quantity, stock.QtyOnHand,
                InventoryLedgerOperation.ForOrder(subId, sku, InventoryChangeType.ReserveRelease),
                "Order", subId, "PaymentTimeout", createdAt: now), ct);
        }

        var flashResult = await flashSales.ReleaseReservedAsync(children.Keys, ct);
        if (flashResult.IsFailure) return flashResult.Error;

        // Reuse the existing cancellation event and payload convention; no new event contract.
        await outbox.AddAsync(new OutboxMessage("OrderCancelledEvent", JsonSerializer.Serialize(new
        {
            parentOrderId = parent.Id, subOrderIds = children.Keys, buyerId = parent.BuyerId,
            changedByRole = "System", reason = "PaymentTimeout", cancelledAt = now
        }), now), ct);
        logger.LogInformation("Expired Payment {PaymentId} and cancelled {SubOrderCount} pending SubOrders",
            payment.Id, children.Count);
        return Result.Success();
    }
}
