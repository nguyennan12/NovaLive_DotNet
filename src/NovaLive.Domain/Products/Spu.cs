using NovaLive.Domain.Common;
using NovaLive.Domain.Products.Events;

namespace NovaLive.Domain.Products;

public sealed class Spu : AuditableEntity, ISoftDeletable
{
    private Spu() { }

    public Spu(
        Guid shopId,
        Guid categoryId,
        string name,
        string? description,
        string? brand,
        string? thumbnailUrl,
        string? attributesConfigJson,
        ProductStatus status = ProductStatus.Active)
    {
        ShopId = shopId;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Brand = brand;
        ThumbnailUrl = thumbnailUrl;
        AttributesConfigJson = attributesConfigJson;
        Status = status;

        Raise(new ProductCreatedDomainEvent(Id, ShopId));
    }

    public Guid ShopId { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Brand { get; private set; }

    public string? ThumbnailUrl { get; private set; }

    public string? AttributesConfigJson { get; private set; }

    public ProductStatus Status { get; private set; } = ProductStatus.Active;

    public DateTimeOffset? DeletedAt { get; private set; }

    public void Update(
        string name,
        string? description,
        Guid categoryId,
        string? brand,
        string? thumbnailUrl,
        string? attributesConfigJson)
    {
        Name = name;
        Description = description;
        CategoryId = categoryId;
        Brand = brand;
        ThumbnailUrl = thumbnailUrl;
        AttributesConfigJson = attributesConfigJson;
        UpdatedAt = DateTimeOffset.UtcNow;

        Raise(new ProductUpdatedDomainEvent(Id, ShopId));
    }

    public void SetStatus(ProductStatus status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;

        Raise(new ProductUpdatedDomainEvent(Id, ShopId));
    }

    public void SoftDelete()
    {
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;

        Raise(new ProductUpdatedDomainEvent(Id, ShopId));
    }
}
