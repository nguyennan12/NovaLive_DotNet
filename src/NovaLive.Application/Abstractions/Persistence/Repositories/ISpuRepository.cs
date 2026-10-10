using NovaLive.Domain.Products;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface ISpuRepository
{
    Task<Spu?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Spu?> GetByIdAndShopAsync(Guid id, Guid shopId, CancellationToken ct = default);
    Task<(List<Spu> Items, long TotalCount)> GetPagedAsync(
        Guid shopId,
        Guid? categoryId,
        string? keyword,
        int page,
        int size,
        CancellationToken ct = default);

    Task AddAsync(Spu spu, CancellationToken ct = default);
    void Update(Spu spu);
    void Delete(Spu spu);
}
