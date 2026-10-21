using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Wallets.Queries.GetWalletTransactions;

public sealed class GetWalletTransactionsQueryHandler(
    IShopWalletRepository wallets,
    IShopWalletTransactionRepository walletTransactions,
    ShopAccess access) : IQueryHandler<GetWalletTransactionsQuery, PagedResult<WalletTransactionResponse>>
{
    public async Task<Result<PagedResult<WalletTransactionResponse>>> Handle(
        GetWalletTransactionsQuery query,
        CancellationToken ct)
    {
        var owned = await access.GetSellerOwnedAsync(false, ct);
        if (owned.IsFailure) return owned.Error;
        var shop = owned.Value!;

        var wallet = await wallets.GetByShopIdAsync(shop.Id, ct);
        var req = query.Request;

        if (wallet is null)
        {
            return PagedResult<WalletTransactionResponse>.Create(
                [],
                req.NormalizedPage,
                req.NormalizedSize,
                0);
        }

        WalletTxType? filterType = null;
        if (!string.IsNullOrWhiteSpace(req.Type) && Enum.TryParse<WalletTxType>(req.Type, true, out var parsedType))
        {
            filterType = parsedType;
        }

        var (transactions, totalCount) = await walletTransactions.GetPagedAsync(
            wallet.Id,
            filterType,
            req.From,
            req.To,
            req.NormalizedPage,
            req.NormalizedSize,
            ct);

        var items = transactions.Select(tx => tx.ToResponse()).ToList();

        return PagedResult<WalletTransactionResponse>.Create(
            items,
            req.NormalizedPage,
            req.NormalizedSize,
            totalCount);
    }
}
