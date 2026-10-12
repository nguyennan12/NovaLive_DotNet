using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class SkuImage : Entity
{
    private SkuImage() { }

    public SkuImage(Guid skuId, string imageUrl, bool isPrimary = false, int displayOrder = 0)
    {
        SkuId = skuId;
        ImageUrl = imageUrl;
        IsPrimary = isPrimary;
        DisplayOrder = displayOrder;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid SkuId { get; private set; }

    public string ImageUrl { get; private set; } = string.Empty;

    public bool IsPrimary { get; private set; }

    public int DisplayOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
