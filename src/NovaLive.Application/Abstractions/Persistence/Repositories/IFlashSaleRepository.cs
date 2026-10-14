using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IFlashSaleRepository
{
    Task<FlashSaleCampaign?> GetCampaignByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<FlashSaleCampaign>> GetActiveCampaignsAsync(DateTimeOffset now, CancellationToken ct = default);

    Task<bool> HasOverlappingFlashSaleAsync(
        Guid skuId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid? excludeCampaignId = null,
        CancellationToken ct = default);

    Task<bool> ExistsItemAsync(Guid campaignId, Guid skuId, CancellationToken ct = default);

    Task<FlashSaleItem?> GetItemByIdAsync(Guid itemId, CancellationToken ct = default);

    Task<(IReadOnlyList<FlashSaleItem> Items, long TotalCount)> GetApprovedItemsByCampaignAsync(
        Guid campaignId,
        int skip,
        int take,
        CancellationToken ct = default);

    Task<(IReadOnlyList<FlashSaleItem> Items, long TotalCount)> GetSellerRegistrationsAsync(
        Guid shopId,
        Guid? campaignId,
        FlashSaleItemStatus? status,
        int skip,
        int take,
        CancellationToken ct = default);

    Task AddCampaignAsync(FlashSaleCampaign campaign, CancellationToken ct = default);

    Task AddItemAsync(FlashSaleItem item, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
