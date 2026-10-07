using NovaLive.Domain.Common;

namespace NovaLive.Domain.Carts;

public sealed class CartItem : Entity
{
    public Guid CartId { get; set; }

    public Guid SkuId { get; set; }

    public Guid ShopId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
