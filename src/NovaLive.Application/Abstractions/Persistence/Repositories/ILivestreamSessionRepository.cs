using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface ILivestreamSessionRepository
{
    Task<LivestreamSession?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<LivestreamSession?> GetByIdAndShopAsync(Guid id, Guid shopId, CancellationToken ct = default);

    Task<LivestreamSession?> GetActiveByShopIdAsync(Guid shopId, CancellationToken ct = default);

    Task<bool> HasActiveLiveSessionAsync(Guid shopId, CancellationToken ct = default);

    Task<(List<LivestreamSession> Items, long TotalCount)> GetActiveSessionsPagedAsync(
        int page,
        int size,
        CancellationToken ct = default);

    Task AddAsync(LivestreamSession session, CancellationToken ct = default);

    void Update(LivestreamSession session);
}
