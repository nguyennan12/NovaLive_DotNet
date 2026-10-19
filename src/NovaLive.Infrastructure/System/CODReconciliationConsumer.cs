using MassTransit;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Shops;

namespace NovaLive.Infrastructure.System;

public sealed class CODReconciliationConsumer(
    IPaymentRepository payments,
    IParentOrderRepository parents,
    ISubOrderRepository subOrders,
    IShopWalletRepository wallets,
    IShopWalletTransactionRepository walletTransactions,
    ITransactionManager transactions,
    IDateTimeProvider clock,
    ILogger<CODReconciliationConsumer> logger) : IConsumer<ShipmentDeliveredEvent>
{
    private const string ReferenceType = "SubOrder";

    public Task Consume(ConsumeContext<ShipmentDeliveredEvent> context)
    {
        if (!context.Message.IsVerifiedWebhook)
        {
            logger.LogDebug("Skipping COD reconciliation for non-webhook delivery event of SubOrder {SubOrderId}.",
                context.Message.SubOrderId);
            return Task.CompletedTask;
        }

        return ReconcileAsync(context);
    }

    private Task ReconcileAsync(ConsumeContext<ShipmentDeliveredEvent> context) =>
        transactions.ExecuteAsync(async () =>
        {
            var message = context.Message;
            var ct = context.CancellationToken;

            // Lock the parent first so settlements for its SubOrders serialize before the
            // parent Payment status is evaluated.
            var parent = await parents.GetForUpdateAsync(message.ParentOrderId, ct)
                ?? throw new InvalidOperationException(
                    $"Shipment delivered event references missing ParentOrder {message.ParentOrderId}.");

            var subOrder = await subOrders.GetForUpdateAsync(message.SubOrderId, ct)
                ?? throw new InvalidOperationException(
                    $"Shipment delivered event references missing SubOrder {message.SubOrderId}.");

            if (subOrder.ParentOrderId != parent.Id || subOrder.ShopId != message.ShopId)
                throw new InvalidOperationException(
                    $"Shipment delivered event references inconsistent order data for SubOrder {message.SubOrderId}.");

            // The verified webhook is accepted as the COD collection confirmation selected
            // for this flow; the event may arrive after polling set DeliveredAt earlier.
            if (subOrder.DeliveredAt is null)
                throw new InvalidOperationException(
                    $"Shipment delivered event references SubOrder {subOrder.Id} without DeliveredAt.");

            var payment = await payments.GetForUpdateAsync(parent.Id, ct);
            if (payment is null)
                throw new InvalidOperationException(
                    $"COD reconciliation cannot find Payment for ParentOrder {parent.Id}.");

            if (payment.Method != PaymentMethod.COD)
            {
                logger.LogDebug("Skipping COD reconciliation for non-COD SubOrder {SubOrderId}.", subOrder.Id);
                return true;
            }

            var alreadyCredited = await walletTransactions.ExistsByReferenceAsync(
                ReferenceType, subOrder.Id, WalletTxType.OrderRevenue, ct);
            if (alreadyCredited)
            {
                logger.LogInformation("COD revenue already recorded for SubOrder {SubOrderId}.", subOrder.Id);
            }
            else
            {
                var wallet = await wallets.GetByShopIdForUpdateAsync(subOrder.ShopId, ct);
                if (wallet is null)
                {
                    wallet = new ShopWallet(subOrder.ShopId);
                    await wallets.AddAsync(wallet, ct);
                }

                // Match the existing online settlement path, which credits SubOrder.SubTotal.
                var revenueAmount = subOrder.SubTotal;
                if (revenueAmount <= 0)
                    throw new InvalidOperationException($"COD SubOrder {subOrder.Id} has a non-positive settlement amount.");
                var now = clock.UtcNow;
                var balanceBefore = wallet.Balance;
                wallet.Credit(revenueAmount, now);

                await walletTransactions.AddAsync(ShopWalletTransaction.Create(
                    walletId: wallet.Id,
                    type: WalletTxType.OrderRevenue,
                    amount: revenueAmount,
                    balanceBefore: balanceBefore,
                    balanceAfter: wallet.Balance,
                    refType: ReferenceType,
                    refId: subOrder.Id,
                    description: $"Doanh thu COD đơn hàng {subOrder.SubOrderCode}",
                    now: now), ct);

                logger.LogInformation(
                    "Recorded COD revenue {Amount} for SubOrder {SubOrderId} and Shop {ShopId}.",
                    revenueAmount, subOrder.Id, subOrder.ShopId);
            }

            // Payment is attached to the ParentOrder. Mark it paid only after every
            // SubOrder has a delivered status and its own revenue ledger entry.
            var parentSubs = await subOrders.GetByParentIdsAsync([parent.Id], tracked: false, ct);
            var creditedSubOrderIds = (await walletTransactions.GetReferenceIdsAsync(
                ReferenceType, parentSubs.Select(s => s.Id), WalletTxType.OrderRevenue, ct)).ToHashSet();
            creditedSubOrderIds.Add(subOrder.Id);
            if (parentSubs.Count > 0 && parentSubs.All(s => s.DeliveredAt is not null && creditedSubOrderIds.Contains(s.Id)))
            {
                var now = clock.UtcNow;
                var paid = parent.MarkPaid(now);
                if (paid.IsFailure)
                    throw new InvalidOperationException($"Unable to mark COD ParentOrder {parent.Id} paid: {paid.Error.Code}.");

                var settled = payment.MarkCodSuccess(now);
                if (settled.IsFailure)
                    throw new InvalidOperationException($"Unable to mark COD Payment for ParentOrder {parent.Id} successful: {settled.Error.Code}.");
            }

            return true;
        }, commitOnFailure: false, context.CancellationToken);
}
