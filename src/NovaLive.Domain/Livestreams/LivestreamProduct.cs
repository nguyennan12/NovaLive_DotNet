using NovaLive.Domain.Common;

namespace NovaLive.Domain.Livestreams;

public sealed class LivestreamProduct : Entity
{
    public Guid SessionId { get; set; }

    public Guid SkuId { get; set; }

    public decimal? FlashPrice { get; set; }

    public int? QuantityLimit { get; set; }

    public int SoldInLive { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPinned { get; set; }

    public DateTimeOffset? PinnedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
