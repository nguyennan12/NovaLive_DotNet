using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface IOrderReturnRepository
{
    Task<OrderReturn?> FindAsync(Guid id, CancellationToken ct);
    Task<List<OrderReturn>> GetBySubOrderIdAsync(Guid subOrderId, CancellationToken ct);
    Task<(List<OrderReturn> Items, long Total)> GetBuyerPagedAsync(Guid buyerId, ReturnStatus? status, int page, int size, CancellationToken ct);
    Task AddAsync(OrderReturn request, CancellationToken ct);
}
