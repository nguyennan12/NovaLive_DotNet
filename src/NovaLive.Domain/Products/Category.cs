using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class Category : Entity
{
    public Guid? ParentId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string? IconUrl { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsVisible { get; private set; } = true;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
