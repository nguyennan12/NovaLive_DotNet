using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class RolePermission : Entity
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;
}
