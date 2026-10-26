using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface ILivestreamCommentRepository
{
    Task AddAsync(LivestreamComment comment, CancellationToken ct = default);

    Task<List<LivestreamComment>> GetRecentBySessionIdAsync(Guid sessionId, int take = 100, CancellationToken ct = default);
}
