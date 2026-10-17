using NovaLive.Domain.Orders;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface IOrderReturnItemRepository
{
    Task<List<OrderReturnItem>> GetByReturnIdsAsync(IEnumerable<Guid> returnIds, CancellationToken ct);
    Task AddAsync(OrderReturnItem item, CancellationToken ct);
}
