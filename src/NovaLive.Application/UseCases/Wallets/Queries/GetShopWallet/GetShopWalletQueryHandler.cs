using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Wallets.Queries.GetShopWallet;

public sealed class GetShopWalletQueryHandler(
    IShopWalletRepository wallets,
    ShopAccess access) : IQueryHandler<GetShopWalletQuery, ShopWalletResponse>
{
    public async Task<Result<ShopWalletResponse>> Handle(GetShopWalletQuery query, CancellationToken ct)
    {
        var owned = await access.GetSellerOwnedAsync(false, ct);
        if (owned.IsFailure) return owned.Error;
        var shop = owned.Value!;

        var wallet = await wallets.GetByShopIdAsync(shop.Id, ct);
        if (wallet is null)
        {
            return new ShopWalletResponse(shop.Id, 0m, 0m, shop.CreatedAt.UtcDateTime);
        }

        return wallet.ToResponse();
    }
}
