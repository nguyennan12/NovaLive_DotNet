namespace NovaLive.Contracts.V1.Livestreams;

public sealed record SellerLivestreamResponse(
    Guid Id,
    string Title,
    string Status,
    string ChannelName,
    int ViewerCount,
    int PeakViewers,
    int TotalOrdersCount,
    decimal TotalRevenue,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? EndedAt);

public sealed record GetSellerLivestreamsRequest(
    int Page = 1,
    int Size = 20,
    string? Status = null);

public sealed record ScheduledLivestreamResponse(
    Guid Id,
    string Title,
    string? ThumbnailUrl,
    string ChannelName,
    string Status,
    DateTimeOffset ScheduledAt);

public sealed record GetSellerLivestreamAnalyticsRequest(string Period = "week");

public sealed record SellerLivestreamAnalyticsResponse(
    int TotalSessions,
    decimal TotalRevenue,
    decimal AveragePeakViewers,
    decimal AverageOrderValue,
    int TotalUnitsSold,
    IReadOnlyList<SellerLivestreamAnalyticsPoint> Points);

public sealed record SellerLivestreamAnalyticsPoint(
    string Label,
    DateTimeOffset PeriodStart,
    decimal Revenue,
    int PeakViewers,
    int UnitsSold);
