using NovaLive.Domain.Common;

namespace NovaLive.Domain.Carts;

public sealed class CartItem : Entity
{
    public Guid CartId { get; private set; }

    public Guid SkuId { get; private set; }

    public Guid ShopId { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public DateTimeOffset AddedAt { get; private set; } = DateTimeOffset.UtcNow;

    public CartItem() { }

    public CartItem(Guid id, Guid cartId, Guid skuId, Guid shopId, int quantity, decimal unitPrice)
        : base(id)
    {
        CartId = cartId;
        SkuId = skuId;
        ShopId = shopId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
