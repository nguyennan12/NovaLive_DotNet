using NovaLive.Domain.Shops;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IShopAddressRepository
{
    Task<List<ShopAddress>> GetPrimaryByShopIdsAsync(IEnumerable<Guid> shopIds, CancellationToken ct = default);
}
