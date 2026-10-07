using NovaLive.Domain.Common;

namespace NovaLive.Domain.Carts;

public sealed class Cart : AuditableEntity
{
    public Guid UserId { get; set; }
}
