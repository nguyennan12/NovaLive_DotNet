using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Products;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class SkuRepository(IAppDbContext dbContext) : ISkuRepository
{
    public async Task<Sku?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Skus
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct);
    }

    public async Task<Sku?> GetByIdAndShopAsync(Guid id, Guid shopId, CancellationToken ct = default)
    {
        return await dbContext.Skus
            .FirstOrDefaultAsync(s => s.Id == id && s.ShopId == shopId && s.DeletedAt == null, ct);
    }

    public async Task<List<Sku>> GetBySpuIdAsync(Guid spuId, CancellationToken ct = default)
    {
        return await dbContext.Skus
            .Where(s => s.SpuId == spuId && s.DeletedAt == null)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, List<Sku>>> GetBySpuIdsAsync(IEnumerable<Guid> spuIds, CancellationToken ct = default)
    {
        var idList = spuIds.Distinct().ToList();
        var skus = await dbContext.Skus
            .AsNoTracking()
            .Where(s => idList.Contains(s.SpuId) && s.DeletedAt == null)
            .ToListAsync(ct);

        return skus
            .GroupBy(s => s.SpuId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<bool> ExistsSkuCodeAsync(Guid shopId, string skuCode, Guid? excludeSkuId = null, CancellationToken ct = default)
    {
        var normalizedCode = skuCode.Trim().ToLower();
        var query = dbContext.Skus
            .AsNoTracking()
            .Where(s => s.ShopId == shopId && s.SkuCode.ToLower() == normalizedCode && s.DeletedAt == null);

        if (excludeSkuId.HasValue)
        {
            query = query.Where(s => s.Id != excludeSkuId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(Sku sku, CancellationToken ct = default)
    {
        await dbContext.Skus.AddAsync(sku, ct);
    }

    public async Task AddRangeAsync(IEnumerable<Sku> skus, CancellationToken ct = default)
    {
        await dbContext.Skus.AddRangeAsync(skus, ct);
    }

    public void Update(Sku sku)
    {
        dbContext.Skus.Update(sku);
    }

    public void Delete(Sku sku)
    {
        dbContext.Skus.Remove(sku);
    }
}
