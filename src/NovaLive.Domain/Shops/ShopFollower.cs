using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class ShopFollower : Entity
{
    public Guid ShopId { get; private set; }

    public Guid UserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
