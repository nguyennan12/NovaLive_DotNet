using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class Role : Entity
{
    private Role() { }

    public Role(
        Guid id,
        string name,
        DateTimeOffset createdAt,
        string? description = null,
        bool isSystem = false)
        : base(id)
    {
        Name = name;
        Description = description;
        IsSystem = isSystem;
        CreatedAt = createdAt;
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsSystem { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public void UpdateSystemDefinition(string name, string description)
    {
        Name = name;
        Description = description;
        IsSystem = true;
    }
}
