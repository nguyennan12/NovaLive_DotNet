using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.Abstractions.Services;

public interface ILivestreamNotificationService
{
    Task BroadcastProductPinnedAsync(Guid sessionId, PinnedProductResponse product, CancellationToken ct = default);

    Task BroadcastProductUnpinnedAsync(Guid sessionId, Guid skuId, CancellationToken ct = default);

    Task BroadcastLiveEndedAsync(Guid sessionId, CancellationToken ct = default);
}
