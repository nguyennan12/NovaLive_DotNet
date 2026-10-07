using NovaLive.Domain.Common;

namespace NovaLive.Domain.Livestreams;

public sealed class LivestreamSession : AuditableEntity
{
    public Guid ShopId { get; set; }

    public Guid HostId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    public string AgoraChannelName { get; set; } = string.Empty;

    public LivestreamStatus Status { get; set; } = LivestreamStatus.Scheduled;

    public int ViewerCount { get; set; }

    public int PeakViewerCount { get; set; }

    public DateTimeOffset? ScheduledAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public string? PlaybackUrl { get; set; }
}
