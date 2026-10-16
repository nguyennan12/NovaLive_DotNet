using NovaLive.Domain.Shipping;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IShippingOrderRepository
{
    Task<ShippingOrder?> FindBySubOrderIdAsync(Guid subOrderId, CancellationToken ct);
    Task<List<ShippingOrder>> GetBySubOrderIdsAsync(IEnumerable<Guid> subOrderIds, CancellationToken ct);
    Task AddAsync(ShippingOrder shipment, CancellationToken ct);
}
