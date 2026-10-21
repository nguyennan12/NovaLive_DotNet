using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Helpers;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Wallets.Commands.CreatePayoutRequest;

public sealed class CreatePayoutRequestCommandHandler(
    IShopWalletRepository wallets,
    IShopWalletTransactionRepository walletTransactions,
    ISellerPayoutRepository payouts,
    IShopRepository shops,
    IUserRepository users,
    IDateTimeProvider clock,
    ShopAccess access) : ICommandHandler<CreatePayoutRequestCommand, SellerPayoutResponse>
{
    public async Task<Result<SellerPayoutResponse>> Handle(
        CreatePayoutRequestCommand command,
        CancellationToken ct)
    {
        var owned = await access.GetSellerOwnedAsync(true, ct);
        if (owned.IsFailure) return owned.Error;
        var shop = owned.Value!;

        var verification = await shops.GetLatestVerificationAsync(shop.Id, ct);
        if (verification is null)
        {
            return WalletErrors.KycRequired;
        }

        var bankAccount = !string.IsNullOrWhiteSpace(command.Request.BankAccount)
            ? command.Request.BankAccount
            : verification.BankAccount;

        var bankName = !string.IsNullOrWhiteSpace(command.Request.BankName)
            ? command.Request.BankName
            : verification.BankName;

        if (string.IsNullOrWhiteSpace(bankAccount) || string.IsNullOrWhiteSpace(bankName))
        {
            return WalletErrors.KycRequired;
        }

        var user = await users.GetByIdAsync(shop.OwnerId, ct);
        var bankHolder = !string.IsNullOrWhiteSpace(command.Request.BankHolder)
            ? command.Request.BankHolder
            : user?.FullName ?? shop.ShopName;

        var wallet = await wallets.GetByShopIdForUpdateAsync(shop.Id, ct);
        if (wallet is null)
        {
            return WalletErrors.NotFound;
        }

        var balanceBefore = wallet.Balance;
        var now = clock.UtcNow;

        var lockResult = wallet.LockPayout(command.Request.Amount, now);
        if (lockResult.IsFailure)
        {
            return lockResult.Error;
        }

        var payout = SellerPayout.Create(
            shop.Id,
            command.Request.Amount,
            bankAccount,
            bankName,
            note: null,
            scheduledAt: now);

        await payouts.AddAsync(payout, ct);

        var tx = ShopWalletTransaction.Create(
            walletId: wallet.Id,
            type: WalletTxType.PayoutLock,
            amount: -command.Request.Amount,
            balanceBefore: balanceBefore,
            balanceAfter: wallet.Balance,
            refType: "SellerPayout",
            refId: payout.Id,
            description: $"Yêu cầu rút tiền về tài khoản {bankAccount} ({bankName})",
            now: now);

        await walletTransactions.AddAsync(tx, ct);

        return payout.ToResponse(bankHolder);
    }
}
