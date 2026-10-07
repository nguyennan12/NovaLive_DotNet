using NovaLive.Domain.Common;

namespace NovaLive.Domain.Livestreams;

public sealed class LivestreamSession : AuditableEntity
{
    public Guid ShopId { get; private set; }

    public Guid HostId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? ThumbnailUrl { get; private set; }

    public string AgoraChannelName { get; private set; } = string.Empty;

    public LivestreamStatus Status { get; private set; } = LivestreamStatus.Scheduled;

    public int ViewerCount { get; private set; }

    public int PeakViewerCount { get; private set; }

    public DateTimeOffset? ScheduledAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public string? PlaybackUrl { get; private set; }
}
