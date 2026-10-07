using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class UserRole : Entity
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;
}
