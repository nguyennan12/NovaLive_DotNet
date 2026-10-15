using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Discounts;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class DiscountRepository(IAppDbContext dbContext) : IDiscountRepository
{
    public async Task<bool> TryConsumeAsync(Guid discountId, Guid userId, DateTimeOffset now, CancellationToken ct)
    {
        // Serializes all consumers of this voucher, including per-user usage checks.
        if (dbContext is DbContext db && db.Database.IsNpgsql())
            await dbContext.Discounts.FromSqlInterpolated(
                $"SELECT * FROM public.discounts WHERE id = {discountId} FOR UPDATE").AsNoTracking().ToListAsync(ct);
        var voucher = await dbContext.Discounts.AsNoTracking().SingleOrDefaultAsync(d => d.Id == discountId, ct);
        if (voucher is null || await GetUserUsageCountAsync(discountId, userId, ct) >= voucher.PerUserLimit) return false;
        return await dbContext.Discounts.Where(d => d.Id == discountId && d.IsActive
                && d.ValidFrom <= now && d.ValidTo >= now && (d.MaxUses == null || d.UsedCount < d.MaxUses))
            .ExecuteUpdateAsync(u => u.SetProperty(d => d.UsedCount, d => d.UsedCount + 1), ct) == 1;
    }
    public async Task AddUsageAsync(DiscountUsage usage, CancellationToken ct) => await dbContext.DiscountUsages.AddAsync(usage, ct);

    public async Task<bool> ExistsCodeAsync(string code, CancellationToken ct = default)
    {
        return await dbContext.Discounts
            .AnyAsync(d => d.Code == code, ct);
    }

    public async Task<Discount?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Discounts
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<Discount?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await dbContext.Discounts
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Code == code, ct);
    }

    public async Task<IReadOnlyList<Discount>> GetPublicShopDiscountsAsync(
        Guid shopId,
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        return await dbContext.Discounts
            .AsNoTracking()
            .Where(d => d.ShopId == shopId &&
                        d.IsPublic &&
                        d.IsActive &&
                        d.ValidTo >= now)
            .OrderBy(d => d.ValidTo)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Discount>> GetPublicPlatformDiscountsAsync(
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        return await dbContext.Discounts
            .AsNoTracking()
            .Where(d => d.ShopId == null &&
                        d.IsPublic &&
                        d.IsActive &&
                        d.ValidTo >= now)
            .OrderBy(d => d.ValidTo)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Discount> Items, long TotalCount)> GetSellerDiscountsAsync(
        Guid shopId,
        string? status,
        int skip,
        int take,
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        var q = dbContext.Discounts
            .AsNoTracking()
            .Where(d => d.ShopId == shopId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = status.Trim().ToLowerInvariant();
            q = s switch
            {
                "active" => q.Where(d => d.IsActive && d.ValidFrom <= now && d.ValidTo >= now),
                "expired" => q.Where(d => d.ValidTo < now),
                "upcoming" => q.Where(d => d.ValidFrom > now),
                "inactive" => q.Where(d => !d.IsActive),
                _ => q
            };
        }

        var total = await q.LongCountAsync(ct);

        var items = await q
            .OrderByDescending(d => d.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Discount discount, CancellationToken ct = default)
    {
        await dbContext.Discounts.AddAsync(discount, ct);
    }

    public void Update(Discount discount)
    {
        dbContext.Discounts.Update(discount);
    }

    public void Delete(Discount discount)
    {
        dbContext.Discounts.Remove(discount);
    }

    public async Task<(IReadOnlyList<Discount> Items, long TotalCount)> GetPlatformDiscountsAdminAsync(
        string? status,
        int skip,
        int take,
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        var q = dbContext.Discounts
            .AsNoTracking()
            .Where(d => d.ShopId == null);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = status.Trim().ToLowerInvariant();
            q = s switch
            {
                "active" => q.Where(d => d.IsActive && d.ValidFrom <= now && d.ValidTo >= now),
                "expired" => q.Where(d => d.ValidTo < now),
                "upcoming" => q.Where(d => d.ValidFrom > now),
                "inactive" => q.Where(d => !d.IsActive),
                _ => q
            };
        }

        var total = await q.LongCountAsync(ct);

        var items = await q
            .OrderByDescending(d => d.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<int> GetUserUsageCountAsync(Guid discountId, Guid userId, CancellationToken ct = default)
    {
        return await dbContext.DiscountUsages
            .CountAsync(u => u.DiscountId == discountId && u.UserId == userId, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
