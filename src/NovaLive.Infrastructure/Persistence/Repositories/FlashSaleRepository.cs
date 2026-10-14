using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class FlashSaleRepository(IAppDbContext dbContext) : IFlashSaleRepository
{
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
            .Where(c => c.Status != FlashSaleCampaignStatus.Cancelled &&
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
