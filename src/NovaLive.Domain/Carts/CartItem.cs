using NovaLive.Domain.Common;

namespace NovaLive.Domain.Carts;

public sealed class CartItem : Entity
{
    public Guid CartId { get; private set; }

    public Guid SkuId { get; private set; }

    public Guid ShopId { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public Guid? LivestreamSessionId { get; private set; }

    public DateTimeOffset AddedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private CartItem() { }

    public CartItem(Guid id, Guid cartId, Guid skuId, Guid shopId, int quantity, decimal unitPrice,
        Guid? livestreamSessionId = null)
        : this(id, cartId, skuId, shopId, quantity, unitPrice, DateTimeOffset.UtcNow, livestreamSessionId) { }

    public CartItem(Guid id, Guid cartId, Guid skuId, Guid shopId, int quantity, decimal unitPrice, DateTimeOffset now,
        Guid? livestreamSessionId = null)
        : base(id)
    {
        CartId = cartId;
        SkuId = skuId;
        ShopId = shopId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LivestreamSessionId = livestreamSessionId;
        AddedAt = UpdatedAt = now;
    }

    public void SetLivestreamSource(Guid? sessionId) => LivestreamSessionId = sessionId;

    public Result IncreaseQuantity(int quantity, DateTimeOffset now)
    {
        if (quantity <= 0 || (long)Quantity + quantity > int.MaxValue)
            return CartDomainErrors.InvalidQuantity;
        return SetQuantity(Quantity + quantity, now);
    }

    public Result SetQuantity(int quantity, DateTimeOffset now)
    {
        if (quantity <= 0) return CartDomainErrors.InvalidQuantity;
        Quantity = quantity;
        UpdatedAt = now;
        return Result.Success();
    }
}
