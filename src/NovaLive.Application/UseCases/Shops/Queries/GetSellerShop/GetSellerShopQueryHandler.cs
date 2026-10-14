using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Queries.GetSellerShop;

public sealed class GetSellerShopQueryHandler(IShopRepository shops, ShopAccess access) : IQueryHandler<GetSellerShopQuery, ShopResponse>
{
    public async Task<Result<ShopResponse>> Handle(GetSellerShopQuery command, CancellationToken ct)
    {
        var owned = await access.GetSellerOwnedAsync(false, ct);
        if (owned.IsFailure) return owned.Error;
        var shop = owned.Value!;
        var verification = await shops.GetLatestVerificationAsync(shop.Id, ct);
        var followers = await shops.CountFollowersAsync(shop.Id, ct);
        return shop.ToResponse(followers, verification);
    }
}
