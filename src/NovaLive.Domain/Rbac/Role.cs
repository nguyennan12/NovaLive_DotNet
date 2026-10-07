using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class Role : Entity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
