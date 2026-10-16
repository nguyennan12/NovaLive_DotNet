using NovaLive.Domain.Common;

namespace NovaLive.Domain.Livestreams;

public sealed class LivestreamProduct : Entity
{
    private LivestreamProduct() { }

    public LivestreamProduct(
        Guid sessionId,
        Guid skuId,
        decimal? flashPrice = null,
        int? quantityLimit = null,
        int displayOrder = 0,
        bool isPinned = false,
        DateTimeOffset? pinnedAt = null)
    {
        SessionId = sessionId;
        SkuId = skuId;
        FlashPrice = flashPrice;
        QuantityLimit = quantityLimit;
        SoldInLive = 0;
        DisplayOrder = displayOrder;
        IsPinned = isPinned;
        PinnedAt = pinnedAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid SessionId { get; private set; }

    public Guid SkuId { get; private set; }

    public decimal? FlashPrice { get; private set; }

    public int? QuantityLimit { get; private set; }

    public int SoldInLive { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsPinned { get; private set; }

    public DateTimeOffset? PinnedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public void Pin(decimal? flashPrice, int? quantityLimit, int displayOrder, DateTimeOffset now)
    {
        FlashPrice = flashPrice;
        QuantityLimit = quantityLimit;
        DisplayOrder = displayOrder;
        IsPinned = true;
        PinnedAt = now;
    }

    public void Unpin()
    {
        IsPinned = false;
    }

    public void RecordSold(int quantity)
    {
        if (quantity > 0)
        {
            SoldInLive += quantity;
        }
    }
}
