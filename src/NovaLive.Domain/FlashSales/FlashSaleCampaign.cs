using NovaLive.Domain.Common;

namespace NovaLive.Domain.FlashSales;

public sealed class FlashSaleCampaign : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    public DateTimeOffset StartAt { get; private set; }

    public DateTimeOffset EndAt { get; private set; }

    public FlashSaleCampaignStatus Status { get; private set; } = FlashSaleCampaignStatus.Scheduled;

    public string? BannerUrl { get; private set; }
}
