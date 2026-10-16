using NovaLive.Domain.Common;

namespace NovaLive.Domain.Livestreams;

public sealed class LivestreamSession : AuditableEntity
{
    private LivestreamSession() { }

    public LivestreamSession(
        Guid shopId,
        Guid hostId,
        string title,
        string? thumbnailUrl,
        string agoraChannelName,
        LivestreamStatus status = LivestreamStatus.Live,
        DateTimeOffset? scheduledAt = null,
        DateTimeOffset? startedAt = null,
        Guid id = default)
    {
        if (id != default)
        {
            Id = id;
        }

        ShopId = shopId;
        HostId = hostId;
        Title = title;
        ThumbnailUrl = thumbnailUrl;
        AgoraChannelName = agoraChannelName;
        Status = status;
        ScheduledAt = scheduledAt;
        StartedAt = startedAt ?? (status == LivestreamStatus.Live ? DateTimeOffset.UtcNow : null);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static LivestreamSession CreateLive(
        Guid shopId,
        Guid hostId,
        string title,
        string? thumbnailUrl,
        string agoraChannelName,
        DateTimeOffset now)
    {
        return new LivestreamSession(shopId, hostId, title, thumbnailUrl, agoraChannelName, LivestreamStatus.Live, startedAt: now);
    }

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

    public Result Start(DateTimeOffset now)
    {
        if (Status == LivestreamStatus.Live)
        {
            return Result.Success();
        }

        if (Status is LivestreamStatus.Ended or LivestreamStatus.Cancelled)
        {
            return LivestreamDomainErrors.InvalidState;
        }

        Status = LivestreamStatus.Live;
        StartedAt = now;
        UpdatedAt = now;
        return Result.Success();
    }

    public Result End(DateTimeOffset now)
    {
        if (Status == LivestreamStatus.Ended)
        {
            return Result.Success();
        }

        if (Status != LivestreamStatus.Live)
        {
            return LivestreamDomainErrors.NotLive;
        }

        Status = LivestreamStatus.Ended;
        EndedAt = now;
        UpdatedAt = now;
        return Result.Success();
    }

    public void UpdateViewerStats(int currentViewers)
    {
        ViewerCount = Math.Max(0, currentViewers);
        if (ViewerCount > PeakViewerCount)
        {
            PeakViewerCount = ViewerCount;
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPlaybackUrl(string? playbackUrl)
    {
        PlaybackUrl = playbackUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
