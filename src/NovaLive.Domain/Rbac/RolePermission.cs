using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class RolePermission : Entity
{
    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId, DateTimeOffset grantedAt)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        GrantedAt = grantedAt;
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

    public DateTimeOffset GrantedAt { get; private set; }
}
