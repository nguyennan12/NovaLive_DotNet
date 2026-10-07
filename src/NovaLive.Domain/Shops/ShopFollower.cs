using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class ShopFollower : Entity
{
    public Guid ShopId { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
