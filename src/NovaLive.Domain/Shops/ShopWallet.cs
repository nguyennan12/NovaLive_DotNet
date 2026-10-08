using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class ShopWallet : Entity
{
    public ShopWallet()
    {
    }

    public ShopWallet(Guid shopId)
    {
        ShopId = shopId;
    }

    public Guid ShopId { get; private set; }

    public decimal Balance { get; private set; }

    public decimal LockedBalance { get; private set; }

    public string Currency { get; private set; } = "VND";

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
