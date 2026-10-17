using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Orders;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class OrderReturnItemRepository(AppDbContext db) : IOrderReturnItemRepository
{
    public Task<List<OrderReturnItem>> GetByReturnIdsAsync(IEnumerable<Guid> returnIds, CancellationToken ct)
    {
        var ids = returnIds.Distinct().ToArray();
        return db.OrderReturnItems.AsNoTracking().Where(i => ids.Contains(i.ReturnId)).OrderBy(i => i.Id).ToListAsync(ct);
    }
    public async Task AddAsync(OrderReturnItem item, CancellationToken ct) => await db.OrderReturnItems.AddAsync(item, ct);
}
