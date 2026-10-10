using NovaLive.Domain.Products;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface ISkuRepository
{
    Task<Sku?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Sku?> GetByIdAndShopAsync(Guid id, Guid shopId, CancellationToken ct = default);
    Task<List<Sku>> GetBySpuIdAsync(Guid spuId, CancellationToken ct = default);
    Task<Dictionary<Guid, List<Sku>>> GetBySpuIdsAsync(IEnumerable<Guid> spuIds, CancellationToken ct = default);
    Task<bool> ExistsSkuCodeAsync(Guid shopId, string skuCode, Guid? excludeSkuId = null, CancellationToken ct = default);
    Task AddAsync(Sku sku, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Sku> skus, CancellationToken ct = default);
    void Update(Sku sku);
    void Delete(Sku sku);
}
