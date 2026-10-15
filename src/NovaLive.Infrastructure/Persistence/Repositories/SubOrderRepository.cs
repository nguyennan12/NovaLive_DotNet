using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Orders;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class SubOrderRepository(AppDbContext db) : ISubOrderRepository
{
    public async Task AddAsync(SubOrder entity, CancellationToken ct) => await db.SubOrders.AddAsync(entity, ct);
    public Task<SubOrder?> FindForShopAsync(Guid id, Guid shopId, CancellationToken ct) =>
        db.SubOrders.AsNoTracking().SingleOrDefaultAsync(s => s.Id == id && s.ShopId == shopId, ct);
    public Task<List<SubOrder>> GetByParentIdsAsync(IEnumerable<Guid> parentIds, bool tracked, CancellationToken ct)
    {
        var ids = parentIds.Distinct().ToArray();
        var query = tracked ? db.SubOrders.AsQueryable() : db.SubOrders.AsNoTracking();
        return query.Where(s => ids.Contains(s.ParentOrderId)).OrderBy(s => s.SubOrderCode).ToListAsync(ct);
    }
    public async Task<(IReadOnlyList<SubOrder> Items, long Total)> GetSellerPagedAsync(Guid shopId,
        NovaLive.Domain.Common.SubOrderStatus? status, DateTimeOffset? from, DateTimeOffset? to, int page, int size, CancellationToken ct)
    {
        var query = db.SubOrders.AsNoTracking().Where(s => s.ShopId == shopId);
        if (status is not null) query = query.Where(s => s.Status == status);
        if (from is not null) query = query.Where(s => s.CreatedAt >= from);
        if (to is not null) query = query.Where(s => s.CreatedAt <= to);
        var count = await query.LongCountAsync(ct);
        return (await query.OrderByDescending(s => s.CreatedAt).ThenByDescending(s => s.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct), count);
    }
}
