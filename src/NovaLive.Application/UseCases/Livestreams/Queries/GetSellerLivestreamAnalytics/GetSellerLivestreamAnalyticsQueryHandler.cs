using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetSellerLivestreamAnalytics;

public sealed class GetSellerLivestreamAnalyticsQueryHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    IDateTimeProvider clock)
    : IQueryHandler<GetSellerLivestreamAnalyticsQuery, SellerLivestreamAnalyticsResponse>
{
    public async Task<Result<SellerLivestreamAnalyticsResponse>> Handle(
        GetSellerLivestreamAnalyticsQuery query,
        CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        var period = query.Request.Period?.Trim().ToLowerInvariant() switch
        {
            "month" => "month",
            "all" => "all",
            _ => "week"
        };
        var now = clock.UtcNow;
        var today = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
        var from = period switch
        {
            "week" => today.AddDays(-6),
            "month" => today.AddDays(-29),
            _ => (DateTimeOffset?)null
        };

        var sessionsQuery = db.LivestreamSessions.AsNoTracking()
            .Where(item => item.ShopId == shopId.Value && item.StartedAt.HasValue);
        if (from.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(item => item.StartedAt >= from.Value);
        }

        var sessions = await sessionsQuery
            .Select(item => new { item.Id, item.StartedAt, item.PeakViewerCount })
            .ToListAsync(ct);
        if (sessions.Count == 0)
        {
            return new SellerLivestreamAnalyticsResponse(0, 0, 0, 0, 0, []);
        }

        var ids = sessions.Select(item => item.Id).ToList();
        var products = await (
            from product in db.LivestreamProducts.AsNoTracking()
            join sku in db.Skus.AsNoTracking() on product.SkuId equals sku.Id into skuGroup
            from sku in skuGroup.DefaultIfEmpty()
            where ids.Contains(product.SessionId)
            select new
            {
                product.SessionId,
                product.SoldInLive,
                UnitPrice = product.FlashPrice ?? (decimal?)sku.SellPrice
            }).ToListAsync(ct);

        var salesBySession = products
            .GroupBy(item => item.SessionId)
            .ToDictionary(
                group => group.Key,
                group => (
                    Units: group.Sum(item => item.SoldInLive),
                    Revenue: group.Sum(item => item.SoldInLive * (item.UnitPrice ?? 0m))));

        var sessionsWithSales = sessions.Select(session =>
        {
            salesBySession.TryGetValue(session.Id, out var sales);
            return new
            {
                session.StartedAt,
                session.PeakViewerCount,
                sales.Units,
                sales.Revenue
            };
        }).ToList();

        var points = sessionsWithSales
            .GroupBy(item =>
            {
                var startedAt = item.StartedAt!.Value.UtcDateTime;
                return period == "all"
                    ? new DateTimeOffset(startedAt.Year, startedAt.Month, 1, 0, 0, 0, TimeSpan.Zero)
                    : new DateTimeOffset(startedAt.Date, TimeSpan.Zero);
            })
            .OrderBy(group => group.Key)
            .Select(group => new SellerLivestreamAnalyticsPoint(
                period == "all" ? group.Key.ToString("MM/yyyy") : group.Key.ToString("dd/MM"),
                group.Key,
                group.Sum(item => item.Revenue),
                group.Max(item => item.PeakViewerCount),
                group.Sum(item => item.Units)))
            .ToList();

        var sessionsCount = sessionsWithSales.Count;
        var totalRevenue = sessionsWithSales.Sum(item => item.Revenue);
        var totalUnits = sessionsWithSales.Sum(item => item.Units);
        return new SellerLivestreamAnalyticsResponse(
            sessionsCount,
            totalRevenue,
            (decimal)sessionsWithSales.Average(item => item.PeakViewerCount),
            totalRevenue / sessionsCount,
            totalUnits,
            points);
    }
}
