using NovaLive.Domain.Shipping;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IShippingOrderRepository
{
    Task<ShippingOrder?> GetByTrackingForUpdateAsync(ShippingProvider provider, string trackingCode, CancellationToken ct);
    Task<List<ShippingOrder>> GetStaleInTransitAsync(DateTimeOffset before, CancellationToken ct);
    Task<ShippingOrder?> FindBySubOrderIdAsync(Guid subOrderId, CancellationToken ct);
    Task<List<ShippingOrder>> GetBySubOrderIdsAsync(IEnumerable<Guid> subOrderIds, CancellationToken ct);
    Task AddAsync(ShippingOrder shipment, CancellationToken ct);
}
