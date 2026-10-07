using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class RolePermission : Entity
{
    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

    public DateTimeOffset GrantedAt { get; private set; } = DateTimeOffset.UtcNow;
}
