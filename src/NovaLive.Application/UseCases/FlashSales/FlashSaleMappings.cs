using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.FlashSales;
using NovaLive.Domain.Products;

namespace NovaLive.Application.UseCases.FlashSales;

public static class FlashSaleMappings
{
    public static FlashSaleCampaignResponse ToResponse(this FlashSaleCampaign campaign) =>
        new(
            campaign.Id,
            campaign.Name,
            campaign.StartAt,
            campaign.EndAt,
            campaign.BannerUrl,
            campaign.Status.ToString());

    public static FlashSaleItemResponse ToResponse(
        this FlashSaleItem item,
        Sku? sku = null,
        Spu? spu = null) =>
        new(
            item.Id,
            item.CampaignId,
            item.SkuId,
            sku?.SpuId ?? Guid.Empty,
            spu?.Name ?? string.Empty,
            sku?.SkuCode ?? string.Empty,
            spu?.ThumbnailUrl ?? string.Empty,
            sku?.OriginalPrice ?? 0m,
            item.FlashPrice,
            item.Quantity,
            item.SoldQty,
            item.ReservedQty,
            item.PerUserLimit,
            item.Status.ToString(),
            item.RejectionReason);
}
