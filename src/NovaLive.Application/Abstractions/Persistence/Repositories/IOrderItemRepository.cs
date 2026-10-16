using NovaLive.Domain.Orders;
using NovaLive.Domain.Common;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface IOrderItemRepository
{
    Task<long> GetFlashSaleUsageAsync(Guid buyerId, Guid flashSaleItemId, CancellationToken ct);

    Task<FlashSaleReservationState?> GetFlashSaleStateAsync(Guid orderItemId, CancellationToken ct);

    Task<bool> TryTransitionFlashSaleAsync(Guid orderItemId, FlashSaleReservationState target, CancellationToken ct);

    Task<List<OrderItem>> GetBySubOrderIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);

    Task AddAsync(OrderItem item, CancellationToken ct);
}
