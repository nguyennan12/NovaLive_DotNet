using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Wallets.Commands.RejectPayout;

public sealed class RejectPayoutCommandHandler(
    ISellerPayoutRepository payouts,
    IShopWalletRepository wallets,
    IShopWalletTransactionRepository walletTransactions,
    IShopRepository shops,
    IUserRepository users,
    ICurrentUser currentUser,
    IDateTimeProvider clock) : ICommandHandler<RejectPayoutCommand, SellerPayoutResponse>
{
    public async Task<Result<SellerPayoutResponse>> Handle(RejectPayoutCommand command, CancellationToken ct)
    {
        var adminId = currentUser.UserId ?? Guid.Empty;
        var payout = await payouts.GetByIdForUpdateAsync(command.PayoutId, ct);
        if (payout is null)
        {
            return WalletErrors.PayoutNotFound;
        }

        if (payout.Status != PayoutStatus.Pending)
        {
            return WalletErrors.PayoutNotPending;
        }

        var wallet = await wallets.GetByShopIdForUpdateAsync(payout.ShopId, ct);
        if (wallet is null)
        {
            return WalletErrors.NotFound;
        }

        var now = clock.UtcNow;

        var unlockResult = wallet.UnlockPayout(payout.Amount, now);
        if (unlockResult.IsFailure)
        {
            return unlockResult.Error;
        }

        payout.Reject(command.Request.Reason, adminId, now);

        var tx = ShopWalletTransaction.Create(
            walletId: wallet.Id,
            type: WalletTxType.PayoutFailedUnlock,
            amount: payout.Amount,
            balanceBefore: wallet.Balance - payout.Amount,
            balanceAfter: wallet.Balance,
            refType: "SellerPayout",
            refId: payout.Id,
            description: $"Từ chối lệnh rút tiền: {command.Request.Reason}",
            now: now);

        await walletTransactions.AddAsync(tx, ct);

        var shop = await shops.GetTrackedByIdAsync(payout.ShopId, ct);
        var owner = shop is not null ? await users.GetByIdAsync(shop.OwnerId, ct) : null;

        return payout.ToResponse(owner?.FullName ?? shop?.ShopName);
    }
}
