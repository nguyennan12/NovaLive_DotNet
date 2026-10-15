using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Orders;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class OrderStatusHistoryRepository(AppDbContext db) : IOrderStatusHistoryRepository
{
    public async Task AddAsync(OrderStatusHistory entity, CancellationToken ct) => await db.OrderStatusHistories.AddAsync(entity, ct);
    public Task<List<OrderStatusHistory>> GetBySubOrderIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var keys = ids.Distinct().ToArray();
        return db.OrderStatusHistories.AsNoTracking().Where(h => keys.Contains(h.SubOrderId))
            .OrderBy(h => h.ChangedAt).ThenBy(h => h.FromStatus == null ? 0 : 1).ThenBy(h => h.Id).ToListAsync(ct);
    }
}
