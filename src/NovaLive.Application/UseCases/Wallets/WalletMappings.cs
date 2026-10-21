using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Wallets;

public static class WalletMappings
{
    public static ShopWalletResponse ToResponse(this ShopWallet wallet) =>
        new(
            wallet.ShopId,
            wallet.Balance,
            wallet.LockedBalance,
            wallet.UpdatedAt.UtcDateTime);

    public static WalletTransactionResponse ToResponse(this ShopWalletTransaction tx) =>
        new(
            tx.Id,
            tx.WalletId,
            tx.Amount,
            tx.Type.ToString(),
            tx.Description ?? string.Empty,
            tx.BalanceAfter,
            tx.CreatedAt.UtcDateTime);

    public static SellerPayoutResponse ToResponse(this SellerPayout payout, string? bankHolder = null) =>
        new(
            payout.Id,
            payout.ShopId,
            payout.Amount,
            payout.BankName,
            payout.BankAccount,
            bankHolder ?? string.Empty,
            payout.Status.ToString(),
            payout.TransferRef,
            payout.Note,
            payout.CreatedAt.UtcDateTime);

    public static PayoutResponse ToPayoutResponse(this SellerPayout payout, string? bankHolder = null) =>
        new(
            payout.Id,
            payout.ShopId,
            payout.Amount,
            payout.BankName,
            payout.BankAccount,
            bankHolder ?? string.Empty,
            payout.Status.ToString(),
            payout.TransferRef,
            payout.Note,
            payout.CreatedAt.UtcDateTime);
}
