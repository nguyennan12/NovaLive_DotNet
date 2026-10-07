using NovaLive.Domain.Common;

namespace NovaLive.Domain.FlashSales;

public sealed class FlashSaleCampaign : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public DateTimeOffset StartAt { get; set; }

    public DateTimeOffset EndAt { get; set; }

    public FlashSaleCampaignStatus Status { get; set; } = FlashSaleCampaignStatus.Scheduled;

    public string? BannerUrl { get; set; }
}
