using NovaLive.Domain.Common;

namespace NovaLive.Domain.FlashSales;

public sealed class FlashSaleCampaign : AuditableEntity
{
    private FlashSaleCampaign() { }

    public FlashSaleCampaign(
        string name,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        string? bannerUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tên chiến dịch Flash Sale không được để trống.", nameof(name));
        }

        if (endAt <= startAt)
        {
            throw new ArgumentException("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.", nameof(endAt));
        }

        Name = name.Trim();
        StartAt = startAt;
        EndAt = endAt;
        BannerUrl = bannerUrl?.Trim();
        Status = FlashSaleCampaignStatus.Scheduled;
    }

    public string Name { get; private set; } = string.Empty;

    public DateTimeOffset StartAt { get; private set; }

    public DateTimeOffset EndAt { get; private set; }

    public FlashSaleCampaignStatus Status { get; private set; } = FlashSaleCampaignStatus.Scheduled;

    public string? BannerUrl { get; private set; }

    public void Cancel()
    {
        Status = FlashSaleCampaignStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsActiveAt(DateTimeOffset time) =>
        Status != FlashSaleCampaignStatus.Cancelled && StartAt <= time && EndAt > time;

    public bool IsUpcomingAt(DateTimeOffset time) =>
        Status != FlashSaleCampaignStatus.Cancelled && time < StartAt;

    public bool IsEndedAt(DateTimeOffset time) =>
        Status != FlashSaleCampaignStatus.Cancelled && time >= EndAt;
}
