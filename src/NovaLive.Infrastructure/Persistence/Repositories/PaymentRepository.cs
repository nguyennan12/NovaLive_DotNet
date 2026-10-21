using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository(AppDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Payments.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Guid>> GetExpiredOnlineIdsAsync(DateTimeOffset now, TimeSpan paymentTimeout,
        int batchSize, CancellationToken ct) =>
        ExpiredOnline(now, paymentTimeout).AsNoTracking().OrderBy(p => p.CreatedAt).ThenBy(p => p.Id)
            .Take(batchSize).Select(p => p.Id).ToListAsync(ct);

    public async Task<bool> TryExpirePendingOnlineAsync(Guid id, DateTimeOffset now,
        TimeSpan paymentTimeout, CancellationToken ct) =>
        await ExpiredOnline(now, paymentTimeout).Where(p => p.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Status, PaymentStatus.Expired)
                .SetProperty(p => p.UpdatedAt, now), ct) == 1;

    private IQueryable<Payment> ExpiredOnline(DateTimeOffset now, TimeSpan paymentTimeout)
    {
        var cutoff = now - paymentTimeout;
        return db.Payments.Where(p => p.Status == PaymentStatus.Pending
            && (p.Method == PaymentMethod.MoMo || p.Method == PaymentMethod.VietQR)
            && (p.ExpiredAt <= now || (p.ExpiredAt == null && p.CreatedAt <= cutoff)));
    }
    public async Task AddAsync(Payment entity, CancellationToken ct) => await db.Payments.AddAsync(entity, ct);

    public Task<List<Payment>> GetByParentIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var keys = ids.Distinct().ToArray();
        return db.Payments.AsNoTracking().Where(p => keys.Contains(p.ParentOrderId)).ToListAsync(ct);
    }

    public async Task<Payment?> GetForUpdateAsync(Guid parentId, CancellationToken ct)
    {
        if (db.Database.IsNpgsql())
            return (await db.Payments.FromSqlInterpolated(
                $"SELECT * FROM public.payments WHERE parent_order_id = {parentId} FOR UPDATE").ToListAsync(ct)).SingleOrDefault();
        return await db.Payments.SingleOrDefaultAsync(p => p.ParentOrderId == parentId, ct);
    }

    public async Task<Payment?> GetByTransactionRefAsync(string transactionRef, CancellationToken ct)
    {
        return await db.Payments.FirstOrDefaultAsync(p => p.TransactionRef == transactionRef, ct);
    }

    public async Task<Payment?> GetByParentOrderIdAsync(Guid parentOrderId, CancellationToken ct)
    {
        return await db.Payments.FirstOrDefaultAsync(p => p.ParentOrderId == parentOrderId, ct);
    }
}
