using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.Abstractions.Services;

public interface ILivestreamCacheManager
{
    Task<int> IncrementViewerCountAsync(Guid sessionId, CancellationToken ct = default);

    Task<int> DecrementViewerCountAsync(Guid sessionId, CancellationToken ct = default);

    Task<int> GetViewerCountAsync(Guid sessionId, CancellationToken ct = default);

    Task AddCommentAsync(Guid sessionId, LivestreamCommentResponse comment, CancellationToken ct = default);

    Task<IReadOnlyList<LivestreamCommentResponse>> GetRecentCommentsAsync(Guid sessionId, int count = 100, CancellationToken ct = default);
}
