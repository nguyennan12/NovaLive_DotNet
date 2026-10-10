using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Products;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class SpuRepository(IAppDbContext dbContext) : ISpuRepository
{
    public async Task<Spu?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Spus
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct);
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
}
