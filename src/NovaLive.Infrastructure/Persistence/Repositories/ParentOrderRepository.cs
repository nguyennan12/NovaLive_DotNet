using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Orders;
using Npgsql;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class ParentOrderRepository(AppDbContext db) : IParentOrderRepository
{
    public async Task<bool> TryAddAsync(ParentOrder order, CancellationToken ct)
    {
        db.ParentOrders.Add(order);
        try { await db.SaveChangesAsync(ct); return true; }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ix_parent_orders_order_code" })
        {
            // EF's automatic savepoint restores this transaction after the unique collision.
            db.Entry(order).State = EntityState.Detached;
            return false;
        }
    }
    public Task<ParentOrder?> FindAsync(Guid id, Guid? buyerId, CancellationToken ct) =>
        db.ParentOrders.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id && (buyerId == null || p.BuyerId == buyerId), ct);
    public async Task<ParentOrder?> GetForUpdateAsync(Guid id, CancellationToken ct)
    {
        if (db.Database.IsNpgsql())
            return (await db.ParentOrders.FromSqlInterpolated(
                $"SELECT * FROM public.parent_orders WHERE id = {id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault();
        return await db.ParentOrders.SingleOrDefaultAsync(p => p.Id == id, ct);
    }
    public async Task<(IReadOnlyList<ParentOrder> Items, long Total)> GetMinePagedAsync(Guid buyerId,
        NovaLive.Domain.Common.ParentOrderPaymentStatus? status, DateTimeOffset? from, DateTimeOffset? to, int page, int size, CancellationToken ct)
    {
        var query = db.ParentOrders.AsNoTracking().Where(p => p.BuyerId == buyerId);
        if (status is not null) query = query.Where(p => p.PaymentStatus == status);
        if (from is not null) query = query.Where(p => p.CreatedAt >= from);
        if (to is not null) query = query.Where(p => p.CreatedAt <= to);
        var count = await query.LongCountAsync(ct);
        return (await query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct), count);
    }
}
