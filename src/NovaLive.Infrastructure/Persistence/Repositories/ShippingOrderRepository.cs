using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Shipping;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class ShippingOrderRepository(AppDbContext db) : IShippingOrderRepository
{
    public Task<ShippingOrder?> FindBySubOrderIdAsync(Guid subOrderId, CancellationToken ct) =>
        db.ShippingOrders.AsNoTracking().SingleOrDefaultAsync(s => s.SubOrderId == subOrderId, ct);

    public Task<List<ShippingOrder>> GetBySubOrderIdsAsync(IEnumerable<Guid> subOrderIds, CancellationToken ct)
    {
        var ids = subOrderIds.Distinct().ToArray();
        return db.ShippingOrders.AsNoTracking().Where(s => ids.Contains(s.SubOrderId))
            .OrderBy(s => s.CreatedAt).ThenBy(s => s.Id).ToListAsync(ct);
    }

    public async Task AddAsync(ShippingOrder shipment, CancellationToken ct) =>
        await db.ShippingOrders.AddAsync(shipment, ct);
}
