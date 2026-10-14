using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Queries.GetMyShop;

public sealed class GetMyShopQueryHandler(IShopRepository shops, ShopAccess access) : IQueryHandler<GetMyShopQuery, ShopResponse>
{
    public async Task<Result<ShopResponse>> Handle(GetMyShopQuery command, CancellationToken ct)
    {
        var owned = await access.GetOwnedAsync(false, ct);
        if (owned.IsFailure) return owned.Error;
        var shop = owned.Value!;
        var verification = await shops.GetLatestVerificationAsync(shop.Id, ct);
        var followers = await shops.CountFollowersAsync(shop.Id, ct);
        return shop.ToResponse(followers, verification);
    }
}
