using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.Abstractions.Services;

public interface ILivestreamNotificationService
{
    Task BroadcastProductPinnedAsync(Guid sessionId, PinnedProductResponse product, CancellationToken ct = default);

    Task BroadcastProductUnpinnedAsync(Guid sessionId, Guid skuId, CancellationToken ct = default);

    Task BroadcastLiveEndedAsync(Guid sessionId, CancellationToken ct = default);

    Task BroadcastViewerCountUpdatedAsync(Guid sessionId, int viewerCount, CancellationToken ct = default);

    Task BroadcastReceiveMessageAsync(Guid sessionId, LivestreamCommentResponse comment, CancellationToken ct = default);

    Task BroadcastReceiveReactionAsync(Guid sessionId, LivestreamReactionBatchResponse reactionBatch, CancellationToken ct = default);
}

