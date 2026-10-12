using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class Category : Entity
{
    private Category() { }

    public Category(
        Guid id,
        string name,
        string slug,
        Guid? parentId = null,
        string? iconUrl = null,
        int displayOrder = 0,
        bool isVisible = true)
    {
        Id = id;
        Name = name;
        Slug = slug;
        ParentId = parentId;
        IconUrl = iconUrl;
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid? ParentId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string? IconUrl { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsVisible { get; private set; } = true;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
