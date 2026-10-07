using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class Resource : Entity
{
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }
}
