using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Shops;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class ShopAddressRepository(AppDbContext dbContext) : IShopAddressRepository
{
    public Task<List<ShopAddress>> GetPrimaryByShopIdsAsync(IEnumerable<Guid> shopIds, CancellationToken ct = default)
    {
        var ids = shopIds.Distinct().ToArray();
        return dbContext.ShopAddresses.AsNoTracking()
            .Where(a => ids.Contains(a.ShopId) && a.IsPrimary).OrderBy(a => a.ShopId).ToListAsync(ct);
    }
}
