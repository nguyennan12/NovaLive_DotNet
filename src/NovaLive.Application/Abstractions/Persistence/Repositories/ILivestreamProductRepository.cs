using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface ILivestreamProductRepository
{
    Task<LivestreamProduct?> GetBySessionAndSkuAsync(Guid sessionId, Guid skuId, CancellationToken ct = default);

    Task<LivestreamProduct?> GetActivePinnedProductAsync(Guid sessionId, CancellationToken ct = default);

    Task<List<LivestreamProduct>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);

    Task AddAsync(LivestreamProduct product, CancellationToken ct = default);

    void Update(LivestreamProduct product);

    void UpdateRange(IEnumerable<LivestreamProduct> products);

    void Delete(LivestreamProduct product);
}
