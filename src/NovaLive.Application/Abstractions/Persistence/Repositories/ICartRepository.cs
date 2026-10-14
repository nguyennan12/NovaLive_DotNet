using NovaLive.Domain.Carts;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<CartItem>> GetItemsAsync(Guid cartId, CancellationToken ct = default);
    Task AddAsync(Cart cart, CancellationToken ct = default);
    Task AddItemAsync(CartItem item, CancellationToken ct = default);
    void RemoveItems(IEnumerable<CartItem> items);
    Task LockUserAsync(Guid userId, CancellationToken ct = default);
    // False only for cart/user or cart/SKU unique races, after savepoint rollback
    // and detaching this cart's state. Reload and reapply the requested increment.
    Task<bool> TrySaveAsync(Guid userId, CancellationToken ct = default);

    Task<List<CartItem>> GetCartItemsByIdsAsync(
        IEnumerable<Guid> cartItemIds,
        Guid? userId,
        CancellationToken ct = default);
}
