using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Shops;
namespace NovaLive.Application.UseCases.Returns.Commands.ConfirmReturnReceived;
public sealed class ConfirmReturnReceivedCommandHandler(SellerReturnAccess access, IOrderReturnItemRepository lines,
    IOrderItemRepository items, IShopWalletRepository wallets, IShopWalletTransactionRepository walletHistory,
    IInventoryRepository inventory, IInventoryHistoryRepository inventoryHistory, IOrderStatusHistoryRepository histories,
    ReturnReader reader, IDateTimeProvider clock) : ICommandHandler<ConfirmReturnReceivedCommand, OrderReturnResponse>
{
    public async Task<Result<OrderReturnResponse>> Handle(ConfirmReturnReceivedCommand command, CancellationToken ct)
    {
        var loaded = await access.GetForUpdateAsync(command.Id, ct);
        if (loaded.IsFailure) return loaded.Error;
        var context = loaded.Value!; var entity = context.Request; var sub = context.SubOrder;
        if (entity.Status != ReturnStatus.SellerApproved || sub.Status != SubOrderStatus.ReturnRequested) return ReturnErrors.InvalidStatus;
        var wallet = await wallets.GetByShopIdForUpdateAsync(sub.ShopId, ct);
        if (wallet is null || wallet.ShopId != sub.ShopId) return ReturnErrors.WalletNotFound;
        if (entity.RefundAmount < 0 || wallet.Balance < entity.RefundAmount) return ReturnErrors.InsufficientShopBalance;
        var returned = await lines.GetByReturnIdsAsync([entity.Id], ct);
        var purchased = (await items.GetBySubOrderIdsAsync([sub.Id], ct)).ToDictionary(i => i.Id);
        if (returned.Count == 0) return ReturnErrors.ItemNotInSubOrder;
        var quantities = new Dictionary<Guid, long>();
        foreach (var line in returned)
        {
            if (!purchased.TryGetValue(line.OrderItemId, out var item) || item.SubOrderId != sub.Id) return ReturnErrors.ItemNotInSubOrder;
            if (line.Quantity <= 0 || line.Quantity > item.Quantity) return ReturnErrors.QuantityExceeded;
            quantities[item.SkuId] = quantities.GetValueOrDefault(item.SkuId) + line.Quantity;
        }
        var stock = (await inventory.GetByIdsForUpdateAsync(quantities.Keys, ct)).ToDictionary(i => i.SkuId);
        foreach (var (sku, quantity) in quantities)
        {
            if (!stock.TryGetValue(sku, out var inv) || inv.ShopId != sub.ShopId) return ReturnErrors.InventoryNotFound;
            if (quantity > int.MaxValue || (long)inv.QtyOnHand + quantity > int.MaxValue || inv.Version == int.MaxValue)
                return ReturnErrors.InvalidInventoryQuantity;
        }
        var now = clock.UtcNow; var before = wallet.Balance;
        var debit = wallet.DebitRefund(entity.RefundAmount, now);
        if (debit.IsFailure) return debit.Error;
        await walletHistory.AddAsync(ShopWalletTransaction.Create(wallet.Id, WalletTxType.OrderRefund, -entity.RefundAmount,
            before, wallet.Balance, "OrderReturn", entity.Id, "Return received", now), ct);
        foreach (var (sku, quantity) in quantities.OrderBy(q => q.Key))
        {
            var inv = stock[sku]; var qtyBefore = inv.QtyOnHand;
            var received = inv.ReceiveReturn((int)quantity, now);
            if (received.IsFailure) return received.Error;
            await inventoryHistory.AddAsync(new InventoryHistory(inv.Id, sku, InventoryChangeType.ReturnIn,
                qtyBefore, (int)quantity, inv.ReservedQty, 0, inv.QtyOnHand, refType: "OrderReturn", refId: entity.Id,
                note: "Return received", createdBy: context.SellerId, createdAt: now), ct);
        }
        var completed = entity.ConfirmReceived(context.SellerId, now);
        if (completed.IsFailure) return completed.Error;
        var transition = sub.MarkAsReturned(now);
        if (transition.IsFailure) return transition.Error;
        await histories.AddAsync(OrderStatusHistory.Create(sub.Id, SubOrderStatus.ReturnRequested, SubOrderStatus.Returned,
            context.SellerId, OrderRole.Seller, "Return received", now), ct);
        return (await reader.DetailAsync(entity, ct))!;
    }
}
