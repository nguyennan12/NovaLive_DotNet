using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Queries.GetFollowedShops;

public sealed class GetFollowedShopsQueryHandler(
    IShopRepository shopRepository,
    ILivestreamSessionRepository sessionRepository,
    ICurrentUser currentUser)
    : IQueryHandler<GetFollowedShopsQuery, List<FollowedShopResponse>>
{
    public async Task<Result<List<FollowedShopResponse>>> Handle(GetFollowedShopsQuery query, CancellationToken ct)
    {
        var shops = new List<NovaLive.Domain.Shops.Shop>();

        if (currentUser.IsAuthenticated && currentUser.UserId is { } userId && userId != Guid.Empty)
        {
            shops = await shopRepository.GetFollowedShopsAsync(userId, ct);
        }

        // If user hasn't followed any shops yet or is guest, suggest active shops
        if (shops.Count == 0)
        {
            shops = await shopRepository.GetSuggestedShopsAsync(10, ct);
        }

        var result = new List<FollowedShopResponse>();
        foreach (var shop in shops)
        {
            var activeLive = await sessionRepository.GetActiveByShopIdAsync(shop.Id, ct);
            result.Add(new FollowedShopResponse(
                shop.Id,
                shop.ShopName,
                shop.Slug,
                shop.LogoUrl,
                shop.Description,
                activeLive != null,
                activeLive?.Id));
        }

        return result;
    }
}
