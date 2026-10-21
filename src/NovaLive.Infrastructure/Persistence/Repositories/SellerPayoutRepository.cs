using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class SellerPayoutRepository(AppDbContext db) : ISellerPayoutRepository
{
    public async Task AddAsync(SellerPayout payout, CancellationToken ct = default)
    {
        await db.SellerPayouts.AddAsync(payout, ct);
    }

    public async Task<SellerPayout?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.SellerPayouts.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<SellerPayout?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        if (db.Database.IsNpgsql())
        {
            return (await db.SellerPayouts.FromSqlInterpolated(
                $"SELECT * FROM public.seller_payouts WHERE id = {id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault();
        }

        return await db.SellerPayouts.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<(List<SellerPayout> Items, long TotalCount)> GetPagedAsync(
        Guid? shopId,
        PayoutStatus? status,
        int page,
        int size,
        CancellationToken ct = default)
    {
        var query = db.SellerPayouts.AsNoTracking();

        if (shopId.HasValue)
        {
            query = query.Where(p => p.ShopId == shopId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var total = await query.LongCountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, total);
    }
}
