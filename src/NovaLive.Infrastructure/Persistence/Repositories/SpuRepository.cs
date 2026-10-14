using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Products;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class SpuRepository(IAppDbContext dbContext) : ISpuRepository
{
    public Task<int> CountActiveByShopAsync(Guid shopId, CancellationToken ct = default) =>
        dbContext.Spus.CountAsync(p => p.ShopId == shopId
            && p.Status == ProductStatus.Active && p.DeletedAt == null, ct);

    public async Task<Spu?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Spus
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct);
    }

    public async Task<List<Spu>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await dbContext.Spus
            .Where(s => idList.Contains(s.Id) && s.DeletedAt == null)
            .ToListAsync(ct);
    }

    public async Task<Spu?> GetByIdAndShopAsync(Guid id, Guid shopId, CancellationToken ct = default)
    {
        return await dbContext.Spus
            .FirstOrDefaultAsync(s => s.Id == id && s.ShopId == shopId && s.DeletedAt == null, ct);
    }

    public async Task<(List<Spu> Items, long TotalCount)> GetPagedAsync(
        Guid shopId,
        Guid? categoryId,
        string? keyword,
        int page,
        int size,
        CancellationToken ct = default)
    {
        var query = dbContext.Spus
            .AsNoTracking()
            .Where(s => s.ShopId == shopId && s.DeletedAt == null);

        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
        {
            query = query.Where(s => s.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(k) || (s.Brand != null && s.Brand.ToLower().Contains(k)));
        }

        var total = await query.LongCountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Spu spu, CancellationToken ct = default)
    {
        await dbContext.Spus.AddAsync(spu, ct);
    }

    public void Update(Spu spu)
    {
        dbContext.Spus.Update(spu);
    }

    public void Delete(Spu spu)
    {
        dbContext.Spus.Remove(spu);
    }

    public async Task<bool> ExistsByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await dbContext.Spus
            .AsNoTracking()
            .AnyAsync(s => s.CategoryId == categoryId && s.DeletedAt == null, ct);
    }

    public async Task<Dictionary<Guid, int>> GetProductCountsByCategoryIdsAsync(IEnumerable<Guid> categoryIds, CancellationToken ct = default)
    {
        var idList = categoryIds.ToList();
        return await dbContext.Spus
            .AsNoTracking()
            .Where(s => idList.Contains(s.CategoryId) && s.DeletedAt == null)
            .GroupBy(s => s.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, ct);
    }
}
