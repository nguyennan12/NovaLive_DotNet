using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class OrderReturnRepository(AppDbContext db) : IOrderReturnRepository
{
    public Task<OrderReturn?> FindAsync(Guid id, CancellationToken ct) => db.OrderReturns.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id, ct);
    public Task<List<OrderReturn>> GetBySubOrderIdAsync(Guid id, CancellationToken ct) => db.OrderReturns.AsNoTracking().Where(r => r.SubOrderId == id).ToListAsync(ct);
    public async Task<(List<OrderReturn> Items, long Total)> GetBuyerPagedAsync(Guid buyerId, ReturnStatus? status, int page, int size, CancellationToken ct)
    {
        var query = db.OrderReturns.AsNoTracking().Where(r => r.BuyerId == buyerId);
        if (status != null) query = query.Where(r => r.Status == status);
        var count = await query.LongCountAsync(ct);
        return (await query.OrderByDescending(r => r.CreatedAt).ThenBy(r => r.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct), count);
    }
    public async Task AddAsync(OrderReturn request, CancellationToken ct) => await db.OrderReturns.AddAsync(request, ct);
}
