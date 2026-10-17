using NovaLive.Domain.Orders;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface ISubOrderRepository
{
    Task<List<Guid>> GetIdsByShopAsync(Guid shopId, CancellationToken ct);
    Task<List<SubOrder>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<SubOrder?> FindByIdAsync(Guid id, CancellationToken ct);
    Task<SubOrder?> GetForUpdateAsync(Guid id, CancellationToken ct);
    Task<SubOrder?> FindForShopAsync(Guid id, Guid shopId, CancellationToken ct);
    Task<List<SubOrder>> GetByParentIdsAsync(IEnumerable<Guid> parentIds, bool tracked, CancellationToken ct);
    Task<(IReadOnlyList<SubOrder> Items, long Total)> GetSellerPagedAsync(Guid shopId,
        NovaLive.Domain.Common.SubOrderStatus? status, DateTimeOffset? from, DateTimeOffset? to, int page, int size, CancellationToken ct);

    Task AddAsync(SubOrder order, CancellationToken ct);
}
