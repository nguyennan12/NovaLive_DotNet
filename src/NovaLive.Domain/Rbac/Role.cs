using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class Role : Entity
{
    private Role() { }

    public Role(string name, string? description = null, bool isSystem = false)
    {
        Name = name;
        Description = description;
        IsSystem = isSystem;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsSystem { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
