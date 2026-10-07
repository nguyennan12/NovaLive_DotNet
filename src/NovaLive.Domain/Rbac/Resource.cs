using NovaLive.Domain.Common;

namespace NovaLive.Domain.Rbac;

public sealed class Resource : Entity
{
    private Resource() { }

    public Resource(string code, string? description = null)
    {
        Code = code;
        Description = description;
    }

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }
}
