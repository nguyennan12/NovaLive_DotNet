using NovaLive.Domain.Shops;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IShopAddressRepository
{
    Task<ShopAddress?> GetReturnAddressAsync(Guid shopId, CancellationToken ct);
    Task<NovaLive.Domain.Shops.ShopAddress?> FindForShopAsync(Guid id, Guid shopId, CancellationToken ct);

    Task<List<ShopAddress>> GetPrimaryByShopIdsAsync(IEnumerable<Guid> shopIds, CancellationToken ct = default);
}
