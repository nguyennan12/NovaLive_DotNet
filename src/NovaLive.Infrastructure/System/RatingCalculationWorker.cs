using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Infrastructure.System;

public sealed class RatingCalculationWorker(IServiceScopeFactory scopeFactory,
    ILogger<RatingCalculationWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private const long AdvisoryLockKey = 724583091;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        try
        {
            do
            {
                try { await RunOnceAsync(stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex) { logger.LogError(ex, "Rating calculation failed; retry on next tick"); }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    public async Task<int> RunOnceAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        if (db.Database.IsNpgsql())
        {
            // One instance calculates at a time; the lock is released on commit, rollback or disconnect.
            var acquired = await db.Database.SqlQuery<bool>(
                $"SELECT pg_try_advisory_xact_lock({AdvisoryLockKey}) AS \"Value\"").SingleAsync(ct);
            if (!acquired) return 0;
        }

        var visibleRatings = db.Reviews.Where(r => r.IsVisible).GroupBy(r => r.ShopId)
            .Select(g => new { ShopId = g.Key, Count = g.Count(), Total = g.Sum(r => (long)r.Rating) });
        var summaries = await (from shop in db.Shops.AsNoTracking()
            join rating in visibleRatings on shop.Id equals rating.ShopId into ratings
            from rating in ratings.DefaultIfEmpty()
            select new
            {
                shop.Id, shop.RatingAvg, shop.RatingCount,
                Count = (int?)rating.Count ?? 0,
                Total = (long?)rating.Total ?? 0
            }).ToListAsync(ct);

        var updated = 0;
        foreach (var summary in summaries)
        {
            var average = summary.Count == 0 ? 0m
                : Math.Round((decimal)summary.Total / summary.Count, 2, MidpointRounding.AwayFromZero);
            if (summary.RatingAvg == average && summary.RatingCount == summary.Count) continue;
            updated += await db.Shops.Where(s => s.Id == summary.Id).ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.RatingAvg, average).SetProperty(s => s.RatingCount, summary.Count), ct);
        }
        // Compare aggregates rather than an in-memory timestamp cursor: edits, moderation, late commits
        // and restarts cannot permanently skip a review. Only changed summaries are written.
        // Reviews on older/soft-deleted SPUs still contribute to their shop's historical rating.
        await transaction.CommitAsync(ct);
        logger.LogInformation("Rating calculation updated {ShopCount} shops", updated);
        return updated;
    }
}
