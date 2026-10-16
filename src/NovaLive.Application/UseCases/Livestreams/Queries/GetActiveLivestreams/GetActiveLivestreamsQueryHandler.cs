using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Livestreams;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetActiveLivestreams;

public sealed class GetActiveLivestreamsQueryHandler(
    ILivestreamSessionRepository sessionRepository,
    IShopRepository shopRepository)
    : IQueryHandler<GetActiveLivestreamsQuery, PagedResult<LivestreamSessionResponse>>
{
    public async Task<Result<PagedResult<LivestreamSessionResponse>>> Handle(
        GetActiveLivestreamsQuery query,
        CancellationToken ct)
    {
        var pagedRequest = new PagedRequest(query.Page, query.Size);

        var (items, total) = await sessionRepository.GetActiveSessionsPagedAsync(
            pagedRequest.NormalizedPage,
            pagedRequest.NormalizedSize,
            ct);

        var shopIds = items.Select(s => s.ShopId).Distinct().ToList();
        var shops = shopIds.Count > 0
            ? await shopRepository.GetByIdsAsync(shopIds, ct)
            : [];

        var shopMap = shops.ToDictionary(s => s.Id);

        var dtos = items.Select(s => s.ToResponse(shopMap.GetValueOrDefault(s.ShopId))).ToList();

        return PagedResult<LivestreamSessionResponse>.Create(
            dtos,
            pagedRequest.NormalizedPage,
            pagedRequest.NormalizedSize,
            total);
    }
}
