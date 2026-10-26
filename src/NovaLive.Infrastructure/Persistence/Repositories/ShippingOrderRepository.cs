using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Shipping;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class ShippingOrderRepository(AppDbContext db) : IShippingOrderRepository
{
    public async Task<ShippingOrder?> GetByTrackingForUpdateAsync(ShippingProvider provider, string trackingCode, CancellationToken ct)
    {
        if (db.Database.IsNpgsql())
            return (await db.ShippingOrders.FromSqlInterpolated(
                $"SELECT * FROM public.shipping_orders WHERE provider = {provider.ToString()} AND tracking_code = {trackingCode} FOR UPDATE")
                .ToListAsync(ct)).SingleOrDefault();
        return await db.ShippingOrders.SingleOrDefaultAsync(s => s.Provider == provider && s.TrackingCode == trackingCode, ct);
    }

    public Task<List<ShippingOrder>> GetStaleInTransitAsync(DateTimeOffset before, CancellationToken ct) =>
        db.ShippingOrders.AsNoTracking().Where(s =>
            (s.Status == ShippingStatus.Picking || s.Status == ShippingStatus.Delivering)
            && s.UpdatedAt < before && s.TrackingCode != null && s.TrackingCode != "")
            .OrderBy(s => s.UpdatedAt).ThenBy(s => s.Id).ToListAsync(ct);

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
