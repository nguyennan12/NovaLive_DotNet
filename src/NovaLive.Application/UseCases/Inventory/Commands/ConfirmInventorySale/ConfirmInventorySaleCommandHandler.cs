using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;

namespace NovaLive.Application.UseCases.Inventory.Commands.ConfirmInventorySale;

public sealed class ConfirmInventorySaleCommandHandler(
    IPaymentRepository payments,
    IParentOrderRepository parents,
    ISubOrderRepository subOrders,
    IOrderItemRepository items,
    IInventoryRepository inventories,
    IInventoryHistoryRepository histories,
    IFlashSaleStockService flashSales,
    IDateTimeProvider clock,
    ILogger<ConfirmInventorySaleCommandHandler> logger) : ICommandHandler<ConfirmInventorySaleCommand>
{
    public async Task<Result> Handle(ConfirmInventorySaleCommand command, CancellationToken ct)
    {
        var payment = await payments.GetByIdAsync(command.PaymentId, ct);
        if (payment?.Status != PaymentStatus.Success) return InventoryErrors.PaymentNotSuccessful;
        var parent = await parents.GetForUpdateAsync(payment.ParentOrderId, ct);
        if (parent is null) return InventoryErrors.OrderNotFound;

        var children = await subOrders.GetByParentIdsAsync([parent.Id], false, ct);
        foreach (var cancelled in children.Where(s => s.Status == SubOrderStatus.Cancelled))
            logger.LogWarning("Late payment {PaymentId} for cancelled SubOrder {SubOrderId}; refund review required",
                payment.Id, cancelled.Id);

        var active = children.Where(s => s.Status != SubOrderStatus.Cancelled).ToDictionary(s => s.Id);
        var lines = await items.GetBySubOrderIdsAsync(active.Keys, ct);
        // Locks serialize ledger checks, including replays after Redis eviction. Keep global SKU order.
        await inventories.GetByIdsForUpdateAsync(lines.Select(i => i.SkuId).Distinct().Order(), ct);
        var now = clock.UtcNow;
        foreach (var group in lines.GroupBy(i => new { i.SkuId, i.SubOrderId })
                     .OrderBy(g => g.Key.SkuId).ThenBy(g => g.Key.SubOrderId))
        {
            var sku = group.Key.SkuId;
            var subId = group.Key.SubOrderId;
            if (await histories.ExistsByReferenceAsync("Order", subId, sku, InventoryChangeType.SaleConfirmed, ct))
                continue;

            var quantity = group.Sum(i => i.Quantity);
            var changed = await inventories.ConfirmReservedSaleAsync(sku, active[subId].ShopId, quantity, now, ct);
            if (changed.IsFailure)
            {
                logger.LogError("Inventory reservation mismatch for Payment {PaymentId}, SubOrder {SubOrderId}, SKU {SkuId}, Quantity {Quantity}",
                    payment.Id, subId, sku, quantity);
                return InventoryErrors.ReservationMismatch;
            }
            var stock = changed.Value!;
            await histories.AddAsync(new InventoryHistory(stock.InventoryId, sku, InventoryChangeType.SaleConfirmed,
                stock.QtyOnHand + quantity, -quantity, stock.ReservedQty + quantity, -quantity, stock.QtyOnHand,
                InventoryLedgerOperation.ForOrder(subId, sku, InventoryChangeType.SaleConfirmed),
                "Order", subId, createdAt: now), ct);
        }

        // T27 owns Flash Sale transitions; its persisted per-line state makes this safe after the webhook.
        var flashResult = await flashSales.ConfirmSoldAsync(active.Keys, ct);
        if (flashResult.IsFailure) return flashResult.Error;
        return Result.Success();
    }
}
