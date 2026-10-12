using NovaLive.Domain.Carts;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface ICartRepository
{
    Task<List<CartItem>> GetCartItemsByIdsAsync(
        IEnumerable<Guid> cartItemIds,
        Guid? userId,
        CancellationToken ct = default);
}
