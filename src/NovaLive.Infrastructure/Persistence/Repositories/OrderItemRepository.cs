using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Common;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class OrderItemRepository(AppDbContext db) : IOrderItemRepository
{
    public Task<OrderItem?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.OrderItems.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, ct);

    public Task<long> GetFlashSaleUsageAsync(Guid buyerId, Guid flashSaleItemId, CancellationToken ct) =>
        (from item in db.OrderItems.AsNoTracking()
         join sub in db.SubOrders.AsNoTracking() on item.SubOrderId equals sub.Id
         join parent in db.ParentOrders.AsNoTracking() on sub.ParentOrderId equals parent.Id
         where parent.BuyerId == buyerId && item.FlashSaleItemId == flashSaleItemId
             && sub.OrderSource == OrderSource.FlashSale && sub.Status != SubOrderStatus.Cancelled
             && (item.FlashSaleState == FlashSaleReservationState.Reserved || item.FlashSaleState == FlashSaleReservationState.Sold)
         select (long)item.Quantity).SumAsync(ct);

    public Task<FlashSaleReservationState?> GetFlashSaleStateAsync(Guid orderItemId, CancellationToken ct) =>
        db.OrderItems.AsNoTracking().Where(i => i.Id == orderItemId).Select(i => i.FlashSaleState).SingleOrDefaultAsync(ct);

    public async Task<bool> TryTransitionFlashSaleAsync(Guid orderItemId, FlashSaleReservationState target, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Flash Sale transitions require the caller's transaction.");
        if (target is not (FlashSaleReservationState.Sold or FlashSaleReservationState.Released)) return false;
        return await db.OrderItems.Where(i => i.Id == orderItemId && i.FlashSaleItemId != null
                && i.FlashSaleState == FlashSaleReservationState.Reserved)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.FlashSaleState, target), ct) == 1;
    }

    public async Task AddAsync(OrderItem entity, CancellationToken ct) => await db.OrderItems.AddAsync(entity, ct);
    public Task<List<OrderItem>> GetBySubOrderIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var keys = ids.Distinct().ToArray();
        return db.OrderItems.AsNoTracking().Where(i => keys.Contains(i.SubOrderId)).OrderBy(i => i.Id).ToListAsync(ct);
    }
}
