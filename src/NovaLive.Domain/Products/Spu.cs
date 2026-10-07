using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class Spu : AuditableEntity
{
    public Guid ShopId { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? AttributesConfigJson { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    public DateTimeOffset? DeletedAt { get; set; }
}
