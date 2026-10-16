using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class LivestreamProductRepository(IAppDbContext dbContext) : ILivestreamProductRepository
{
    public async Task<LivestreamProduct?> GetBySessionAndSkuAsync(Guid sessionId, Guid skuId, CancellationToken ct = default)
    {
        return await dbContext.LivestreamProducts
            .FirstOrDefaultAsync(p => p.SessionId == sessionId && p.SkuId == skuId, ct);
    }

    public async Task<LivestreamProduct?> GetActivePinnedProductAsync(Guid sessionId, CancellationToken ct = default)
    {
        return await dbContext.LivestreamProducts
            .FirstOrDefaultAsync(p => p.SessionId == sessionId && p.IsPinned, ct);
    }

    public async Task<List<LivestreamProduct>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        return await dbContext.LivestreamProducts
            .Where(p => p.SessionId == sessionId)
            .OrderBy(p => p.DisplayOrder)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(LivestreamProduct product, CancellationToken ct = default)
    {
        await dbContext.LivestreamProducts.AddAsync(product, ct);
    }

    public void Update(LivestreamProduct product)
    {
        dbContext.LivestreamProducts.Update(product);
    }

    public void UpdateRange(IEnumerable<LivestreamProduct> products)
    {
        dbContext.LivestreamProducts.UpdateRange(products);
    }

    public void Delete(LivestreamProduct product)
    {
        dbContext.LivestreamProducts.Remove(product);
    }
}
