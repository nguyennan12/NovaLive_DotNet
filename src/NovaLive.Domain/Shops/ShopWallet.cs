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

    public Guid ShopId { get; set; }

    public decimal Balance { get; set; }

    public decimal HoldingBalance { get; set; }

    public decimal LockedBalance { get; set; }

    public string Currency { get; set; } = "VND";

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
