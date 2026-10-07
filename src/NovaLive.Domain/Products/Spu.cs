using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class Spu : AuditableEntity
{
    public Guid ShopId { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Brand { get; private set; }

    public string? ThumbnailUrl { get; private set; }

    public string? AttributesConfigJson { get; private set; }

    public ProductStatus Status { get; private set; } = ProductStatus.Draft;

    public DateTimeOffset? DeletedAt { get; private set; }
}
