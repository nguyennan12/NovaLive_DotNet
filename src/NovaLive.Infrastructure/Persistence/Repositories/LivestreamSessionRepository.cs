using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class LivestreamSessionRepository(IAppDbContext dbContext) : ILivestreamSessionRepository
{
    public async Task<LivestreamSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.LivestreamSessions
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<LivestreamSession?> GetByIdAndShopAsync(Guid id, Guid shopId, CancellationToken ct = default)
    {
        return await dbContext.LivestreamSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.ShopId == shopId, ct);
    }

    public async Task<LivestreamSession?> GetActiveByShopIdAsync(Guid shopId, CancellationToken ct = default)
    {
        return await dbContext.LivestreamSessions
            .FirstOrDefaultAsync(s => s.ShopId == shopId && s.Status == LivestreamStatus.Live, ct);
    }

    public async Task<bool> HasActiveLiveSessionAsync(Guid shopId, CancellationToken ct = default)
    {
        return await dbContext.LivestreamSessions
            .AsNoTracking()
            .AnyAsync(s => s.ShopId == shopId && s.Status == LivestreamStatus.Live, ct);
    }

    public async Task<(List<LivestreamSession> Items, long TotalCount)> GetActiveSessionsPagedAsync(
        int page,
        int size,
        CancellationToken ct = default)
    {
        var query = dbContext.LivestreamSessions
            .AsNoTracking()
            .Where(s => s.Status == LivestreamStatus.Live)
            .OrderByDescending(s => s.ViewerCount)
            .ThenByDescending(s => s.StartedAt);

        var total = await query.LongCountAsync(ct);
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(LivestreamSession session, CancellationToken ct = default)
    {
        await dbContext.LivestreamSessions.AddAsync(session, ct);
    }

    public void Update(LivestreamSession session)
    {
        dbContext.LivestreamSessions.Update(session);
    }
}
