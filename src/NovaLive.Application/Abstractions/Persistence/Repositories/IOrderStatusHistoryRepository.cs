using NovaLive.Domain.Orders;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface IOrderStatusHistoryRepository
{
    Task<List<OrderStatusHistory>> GetBySubOrderIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);

    Task AddAsync(OrderStatusHistory history, CancellationToken ct);
}
