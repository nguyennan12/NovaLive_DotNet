using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Shops;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class ShopAddressRepository(AppDbContext dbContext) : IShopAddressRepository
{
    public Task<ShopAddress?> GetReturnAddressAsync(Guid shopId, CancellationToken ct) =>
        dbContext.ShopAddresses.AsNoTracking().Where(a => a.ShopId == shopId && a.IsReturn)
            .OrderBy(a => a.CreatedAt).ThenBy(a => a.Id).FirstOrDefaultAsync(ct);
    public Task<ShopAddress?> FindForShopAsync(Guid id, Guid shopId, CancellationToken ct) =>
        dbContext.ShopAddresses.AsNoTracking().SingleOrDefaultAsync(a => a.Id == id && a.ShopId == shopId, ct);

    public Task<List<ShopAddress>> GetPrimaryByShopIdsAsync(IEnumerable<Guid> shopIds, CancellationToken ct = default)
    {
        var ids = shopIds.Distinct().ToArray();
        return dbContext.ShopAddresses.AsNoTracking()
            .Where(a => ids.Contains(a.ShopId) && a.IsPrimary).OrderBy(a => a.ShopId).ToListAsync(ct);
    }
}
