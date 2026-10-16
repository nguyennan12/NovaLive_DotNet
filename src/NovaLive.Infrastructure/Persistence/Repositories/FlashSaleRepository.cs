using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class FlashSaleRepository(AppDbContext dbContext) : IFlashSaleRepository
{
    public async Task<IReadOnlyList<FlashSaleItem>> GetActiveItemsBySkuIdsAsync(
        IEnumerable<Guid> skuIds, DateTimeOffset now, CancellationToken ct = default)
    {
        var ids = skuIds.Distinct().ToArray();
        return await dbContext.FlashSaleItems.AsNoTracking().Where(i => ids.Contains(i.SkuId)
            && i.Status == FlashSaleItemStatus.Approved
            && dbContext.FlashSaleCampaigns.Any(c => c.Id == i.CampaignId
                && (c.Status == FlashSaleCampaignStatus.Active || c.Status == FlashSaleCampaignStatus.Scheduled)
                && c.StartAt <= now && c.EndAt > now))
            .OrderBy(i => i.Id).ToListAsync(ct);
    }

    public async Task LockBuyerItemAsync(Guid buyerId, Guid flashSaleItemId, CancellationToken ct = default)
    {
        RequireTransaction();
        if (dbContext.Database.IsNpgsql())
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({$"flash-sale:{buyerId:D}:{flashSaleItemId:D}"}, 827));", ct);
    }

    public async Task<bool> TryReserveAsync(Guid flashSaleItemId, int quantity, CancellationToken ct = default)
    {
        RequireTransaction();
        if (quantity <= 0) return false;
        // One parameterized UPDATE; EXISTS stays inside the Flash Sale module.
        // T26 derives Active from the time window, even before Scheduled is persisted as Active.
        // Use the database clock so a long transaction cannot reserve using its earlier start time.
        return await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE flash_sale_items SET reserved_qty = reserved_qty + {quantity}
            WHERE id = {flashSaleItemId} AND status = 'Approved'
              AND (quantity - (reserved_qty + sold_qty)) >= {quantity}
              AND EXISTS (SELECT 1 FROM flash_sale_campaigns c
                  WHERE c.id = flash_sale_items.campaign_id AND c.status IN ('Active', 'Scheduled')
                    AND c.start_at <= clock_timestamp() AND c.end_at > clock_timestamp())
            """, ct) == 1;
    }

    public async Task<bool> ConfirmSoldAsync(Guid flashSaleItemId, int quantity, CancellationToken ct = default)
    {
        RequireTransaction();
        if (quantity <= 0) return false;
        return await dbContext.FlashSaleItems.Where(i => i.Id == flashSaleItemId && i.ReservedQty >= quantity)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.ReservedQty, i => i.ReservedQty - quantity)
                .SetProperty(i => i.SoldQty, i => i.SoldQty + quantity), ct) == 1;
    }

    public async Task<bool> ReleaseReservedAsync(Guid flashSaleItemId, int quantity, CancellationToken ct = default)
    {
        RequireTransaction();
        if (quantity <= 0) return false;
        return await dbContext.FlashSaleItems.Where(i => i.Id == flashSaleItemId && i.ReservedQty >= quantity)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.ReservedQty, i => i.ReservedQty - quantity), ct) == 1;
    }

    private void RequireTransaction()
    {
        if (dbContext.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Flash Sale stock operations require the caller's transaction.");
    }

    public async Task<FlashSaleCampaign?> GetCampaignByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.FlashSaleCampaigns
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<FlashSaleCampaign>> GetActiveCampaignsAsync(
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        return await dbContext.FlashSaleCampaigns
            .AsNoTracking()
            .Where(c => (c.Status == FlashSaleCampaignStatus.Active || c.Status == FlashSaleCampaignStatus.Scheduled) &&
                        c.StartAt <= now &&
                        c.EndAt > now)
            .OrderBy(c => c.StartAt)
            .ToListAsync(ct);
    }

    public async Task<bool> HasOverlappingFlashSaleAsync(
        Guid skuId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid? excludeCampaignId = null,
        CancellationToken ct = default)
    {
        var query = from item in dbContext.FlashSaleItems.AsNoTracking()
                    join campaign in dbContext.FlashSaleCampaigns.AsNoTracking()
                        on item.CampaignId equals campaign.Id
                    where item.SkuId == skuId &&
                          item.Status != FlashSaleItemStatus.Rejected &&
                          item.Status != FlashSaleItemStatus.Ended &&
                          campaign.Status != FlashSaleCampaignStatus.Cancelled &&
                          campaign.StartAt < endAt &&
                          campaign.EndAt > startAt
                    select item;

        if (excludeCampaignId.HasValue && excludeCampaignId.Value != Guid.Empty)
        {
            query = query.Where(i => i.CampaignId != excludeCampaignId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<bool> ExistsItemAsync(Guid campaignId, Guid skuId, CancellationToken ct = default)
    {
        return await dbContext.FlashSaleItems
            .AnyAsync(i => i.CampaignId == campaignId && i.SkuId == skuId, ct);
    }

    public async Task<FlashSaleItem?> GetItemByIdAsync(Guid itemId, CancellationToken ct = default)
    {
        return await dbContext.FlashSaleItems
            .FirstOrDefaultAsync(i => i.Id == itemId, ct);
    }

    public async Task<(IReadOnlyList<FlashSaleItem> Items, long TotalCount)> GetApprovedItemsByCampaignAsync(
        Guid campaignId,
        int skip,
        int take,
        CancellationToken ct = default)
    {
        var query = dbContext.FlashSaleItems
            .AsNoTracking()
            .Where(i => i.CampaignId == campaignId && i.Status == FlashSaleItemStatus.Approved);

        var totalCount = await query.LongCountAsync(ct);
        var items = await query
            .OrderBy(i => i.FlashPrice)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<FlashSaleItem> Items, long TotalCount)> GetSellerRegistrationsAsync(
        Guid shopId,
        Guid? campaignId,
        FlashSaleItemStatus? status,
        int skip,
        int take,
        CancellationToken ct = default)
    {
        var query = dbContext.FlashSaleItems
            .AsNoTracking()
            .Where(i => i.ShopId == shopId);

        if (campaignId.HasValue && campaignId.Value != Guid.Empty)
        {
            query = query.Where(i => i.CampaignId == campaignId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        var totalCount = await query.LongCountAsync(ct);
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddCampaignAsync(FlashSaleCampaign campaign, CancellationToken ct = default)
    {
        await dbContext.FlashSaleCampaigns.AddAsync(campaign, ct);
    }

    public async Task AddItemAsync(FlashSaleItem item, CancellationToken ct = default)
    {
        await dbContext.FlashSaleItems.AddAsync(item, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
