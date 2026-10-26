using Microsoft.AspNetCore.SignalR;
using NovaLive.Api.Hubs;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Api.Services;

public sealed class LivestreamNotificationService(IHubContext<LivestreamHub> hubContext) : ILivestreamNotificationService
{
    public async Task BroadcastProductPinnedAsync(Guid sessionId, PinnedProductResponse product, CancellationToken ct = default)
    {
        await hubContext.Clients.Group($"live_{sessionId:N}").SendAsync("ProductPinned", product, ct);
    }

    public async Task BroadcastProductUnpinnedAsync(Guid sessionId, Guid skuId, CancellationToken ct = default)
    {
        await hubContext.Clients.Group($"live_{sessionId:N}").SendAsync("ProductUnpinned", new { sessionId, skuId }, ct);
    }

    public async Task BroadcastLiveEndedAsync(Guid sessionId, CancellationToken ct = default)
    {
        await hubContext.Clients.Group($"live_{sessionId:N}").SendAsync("LiveEnded", new { sessionId, endedAt = DateTimeOffset.UtcNow }, ct);
    }

    public async Task BroadcastViewerCountUpdatedAsync(Guid sessionId, int viewerCount, CancellationToken ct = default)
    {
        await hubContext.Clients.Group($"live_{sessionId:N}").SendAsync("ViewerCountUpdated", new ViewerCountUpdatedResponse(sessionId, viewerCount), ct);
    }

    public async Task BroadcastReceiveMessageAsync(Guid sessionId, LivestreamCommentResponse comment, CancellationToken ct = default)
    {
        await hubContext.Clients.Group($"live_{sessionId:N}").SendAsync("ReceiveMessage", comment, ct);
    }

    public async Task BroadcastReceiveReactionAsync(Guid sessionId, LivestreamReactionBatchResponse reactionBatch, CancellationToken ct = default)
    {
        await hubContext.Clients.Group($"live_{sessionId:N}").SendAsync("ReceiveReaction", reactionBatch, ct);
    }
}
