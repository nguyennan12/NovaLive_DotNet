using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetSellerLivestreams;

public sealed class GetSellerLivestreamsQueryHandler(
    IAppDbContext db,
    ICurrentUser currentUser)
    : IQueryHandler<GetSellerLivestreamsQuery, PagedResult<SellerLivestreamResponse>>
{
    public async Task<Result<PagedResult<SellerLivestreamResponse>>> Handle(GetSellerLivestreamsQuery query, CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        var request = query.Request;
        var page = Math.Max(1, request.Page);
        var size = request.Size <= 0 ? 20 : Math.Min(request.Size, 100);
        var sessions = db.LivestreamSessions.AsNoTracking().Where(item => item.ShopId == shopId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<LivestreamStatus>(NormalizeStatus(request.Status), true, out var status))
        {
            sessions = sessions.Where(item => item.Status == status);
        }

        var total = await sessions.LongCountAsync(ct);
        var rows = await sessions
            .OrderByDescending(item => item.CreatedAt)
            .ThenByDescending(item => item.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(item => new
            {
                item.Id,
                item.Title,
                Status = item.Status.ToString(),
                item.AgoraChannelName,
                item.ViewerCount,
                item.PeakViewerCount,
                item.ScheduledAt,
                item.StartedAt,
                item.EndedAt
            })
            .ToListAsync(ct);

        var ids = rows.Select(item => item.Id).ToList();
        var stats = ids.Count == 0
            ? new Dictionary<Guid, (int Orders, decimal Revenue)>()
            : await db.LivestreamProducts.AsNoTracking()
                .Where(item => ids.Contains(item.SessionId))
                .GroupBy(item => item.SessionId)
                .Select(group => new
                {
                    SessionId = group.Key,
                    Orders = group.Sum(item => item.SoldInLive),
                    Revenue = group.Sum(item => item.SoldInLive * (item.FlashPrice ?? 0m))
                })
                .ToDictionaryAsync(item => item.SessionId, item => (item.Orders, item.Revenue), ct);

        var response = rows.Select(item =>
        {
            stats.TryGetValue(item.Id, out var totalStats);
            return new SellerLivestreamResponse(
                item.Id,
                item.Title,
                item.Status,
                item.AgoraChannelName,
                item.ViewerCount,
                item.PeakViewerCount,
                totalStats.Orders,
                totalStats.Revenue,
                item.ScheduledAt,
                item.StartedAt,
                item.EndedAt);
        }).ToList();

        return PagedResult<SellerLivestreamResponse>.Create(response, page, size, total);
    }

    private static string NormalizeStatus(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "canceled" => nameof(LivestreamStatus.Cancelled),
            "cancelled" => nameof(LivestreamStatus.Cancelled),
            _ => status.Trim()
        };
}
