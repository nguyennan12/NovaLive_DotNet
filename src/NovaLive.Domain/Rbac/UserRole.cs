using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class UserRole : Entity
{
    private UserRole() { }

    public UserRole(Guid userId, Guid roleId, DateTimeOffset grantedAt)
    {
        UserId = userId;
        RoleId = roleId;
        GrantedAt = grantedAt;
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public DateTimeOffset GrantedAt { get; private set; }
}
