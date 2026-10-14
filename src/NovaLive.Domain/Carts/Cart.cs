using NovaLive.Domain.Common;

namespace NovaLive.Domain.Carts;

public sealed class Cart : Entity
{
    private Cart() { }

    public Cart(Guid userId, DateTimeOffset now)
    {
        UserId = userId;
        CreatedAt = UpdatedAt = now;
        CreatedBy = UpdatedBy = userId;
    }

    public Guid UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public Result<CartItem> AddItem(IReadOnlyCollection<CartItem> items, Guid skuId, Guid shopId,
        int quantity, decimal unitPrice, DateTimeOffset now)
    {
        if (quantity <= 0) return CartDomainErrors.InvalidQuantity;
        if (unitPrice < 0) return CartDomainErrors.InvalidPrice;
        var item = items.SingleOrDefault(i => i.CartId == Id && i.SkuId == skuId);
        if (item is null)
            item = new CartItem(Guid.NewGuid(), Id, skuId, shopId, quantity, unitPrice, now);
        else
        {
            var result = item.IncreaseQuantity(quantity, now);
            if (result.IsFailure) return result.Error;
        }
        Touch(now);
        return item;
    }

    public Result UpdateItem(CartItem item, int quantity, DateTimeOffset now)
    {
        if (item.CartId != Id) return CartDomainErrors.ItemNotFound;
        var result = item.SetQuantity(quantity, now);
        if (result.IsSuccess) Touch(now);
        return result;
    }

    // Callers hard-delete exactly these owned items in their existing transaction.
    // Keeping the selection here lets checkout reuse it without invoking a cart command.
    public List<CartItem> RemoveSelectedItems(IReadOnlyCollection<CartItem> items,
        IEnumerable<Guid> selectedIds, DateTimeOffset now)
    {
        var ids = selectedIds.ToHashSet();
        var removed = items.Where(i => i.CartId == Id && ids.Contains(i.Id)).ToList();
        if (removed.Count > 0) Touch(now);
        return removed;
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
        UpdatedBy = UserId;
    }
}
