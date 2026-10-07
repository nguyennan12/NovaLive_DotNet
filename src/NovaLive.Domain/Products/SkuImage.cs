using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class SkuImage : Entity
{
    public Guid SkuId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
