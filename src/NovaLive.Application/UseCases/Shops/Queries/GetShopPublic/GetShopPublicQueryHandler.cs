using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Queries.GetShopPublic;

public sealed class GetShopPublicQueryHandler(IShopRepository shops, ISpuRepository productsRepository, ICurrentUser currentUser) : IQueryHandler<GetShopPublicQuery, ShopDetailResponse>
{
    public async Task<Result<ShopDetailResponse>> Handle(GetShopPublicQuery command, CancellationToken ct)
    {
        var shop = await shops.GetActiveByIdAsync(command.ShopId, ct);
        if (shop is null) return ShopErrors.NotFound;
        var followers = await shops.CountFollowersAsync(shop.Id, ct);
        var products = await productsRepository.CountActiveByShopAsync(shop.Id, ct);
        var following = currentUser.IsAuthenticated && currentUser.UserId.HasValue
            && await shops.IsFollowingAsync(shop.Id, currentUser.UserId, ct);
        var addresses = await shops.GetPublicAddressesAsync(shop.Id, ct);
        // Explicit public contract projection; never load verification/bank data here.
        return new ShopDetailResponse(shop.Id, shop.OwnerId, shop.ShopName, shop.Slug, shop.Description ?? "",
            shop.LogoUrl, shop.BannerUrl, shop.Phone ?? "", shop.Email ?? "", shop.Status.ToString(),
            (double)shop.RatingAvg, followers, shop.CreatedAt.UtcDateTime,
            addresses.Select(a => a.ToResponse()).ToList(), shop.RatingCount, products, following);
    }
}
