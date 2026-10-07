using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class SkuImage : Entity
{
    public Guid SkuId { get; private set; }

    public string ImageUrl { get; private set; } = string.Empty;

    public bool IsPrimary { get; private set; }

    public int DisplayOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
