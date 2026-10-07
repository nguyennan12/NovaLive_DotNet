using NovaLive.Domain.Common;

namespace NovaLive.Domain.Livestreams;

public sealed class LivestreamProduct : Entity
{
    public Guid SessionId { get; private set; }

    public Guid SkuId { get; private set; }

    public decimal? FlashPrice { get; private set; }

    public int? QuantityLimit { get; private set; }

    public int SoldInLive { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsPinned { get; private set; }

    public DateTimeOffset? PinnedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
