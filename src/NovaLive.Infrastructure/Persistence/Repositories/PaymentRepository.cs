using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Payments;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository(AppDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Payments.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);
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
