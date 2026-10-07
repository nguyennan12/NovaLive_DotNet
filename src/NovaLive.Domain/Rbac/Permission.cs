using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class Permission : Entity
{
    private Permission() { }

    public Permission(Guid resourceId, RbacAction action, string code, string? description = null)
    {
        ResourceId = resourceId;
        Action = action;
        Code = code;
        Description = description;
    }

    public Guid ResourceId { get; private set; }

    public RbacAction Action { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }
}
