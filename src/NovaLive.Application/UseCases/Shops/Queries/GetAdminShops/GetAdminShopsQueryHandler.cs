using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops.Queries.GetAdminShops;

public sealed class GetAdminShopsQueryHandler(
    IShopRepository shops) : IQueryHandler<GetAdminShopsQuery, PagedResult<ShopResponse>>
{
    public async Task<Result<PagedResult<ShopResponse>>> Handle(GetAdminShopsQuery query, CancellationToken ct)
    {
        var req = query.Request;
        ShopStatus? filterStatus = null;
        if (!string.IsNullOrWhiteSpace(req.Status) && Enum.TryParse<ShopStatus>(req.Status, true, out var parsedStatus))
        {
            filterStatus = parsedStatus;
        }

        var (items, totalCount) = await shops.GetPagedAsync(
            filterStatus,
            req.NormalizedPage,
            req.NormalizedSize,
            ct);

        var responses = new List<ShopResponse>();
        foreach (var shop in items)
        {
            var verification = await shops.GetLatestVerificationAsync(shop.Id, ct);
            var followers = await shops.CountFollowersAsync(shop.Id, ct);
            responses.Add(shop.ToResponse(followers, verification));
        }

        return PagedResult<ShopResponse>.Create(
            responses,
            req.NormalizedPage,
            req.NormalizedSize,
            totalCount);
    }
}
