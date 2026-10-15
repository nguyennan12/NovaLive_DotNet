using NovaLive.Domain.Orders;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface IOrderItemRepository
{
    Task<List<OrderItem>> GetBySubOrderIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);

    Task AddAsync(OrderItem item, CancellationToken ct);
}
