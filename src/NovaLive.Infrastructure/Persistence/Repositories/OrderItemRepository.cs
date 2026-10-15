using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Orders;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class OrderItemRepository(AppDbContext db) : IOrderItemRepository
{
    public async Task AddAsync(OrderItem entity, CancellationToken ct) => await db.OrderItems.AddAsync(entity, ct);
    public Task<List<OrderItem>> GetBySubOrderIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var keys = ids.Distinct().ToArray();
        return db.OrderItems.AsNoTracking().Where(i => keys.Contains(i.SubOrderId)).OrderBy(i => i.Id).ToListAsync(ct);
    }
}
