using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class LivestreamCommentRepository(IAppDbContext dbContext) : ILivestreamCommentRepository
{
    public async Task AddAsync(LivestreamComment comment, CancellationToken ct = default)
    {
        await dbContext.LivestreamComments.AddAsync(comment, ct);
    }

    public async Task<List<LivestreamComment>> GetRecentBySessionIdAsync(Guid sessionId, int take = 100, CancellationToken ct = default)
    {
        return await dbContext.LivestreamComments
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }
}
