using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class Permission : Entity
{
    public Guid ResourceId { get; set; }

    public RbacAction Action { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }
}
