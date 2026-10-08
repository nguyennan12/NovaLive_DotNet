namespace NovaLive.Contracts.V1.FlashSales;

public record FlashSaleCampaignResponse(
    Guid Id,
    string Name,
    DateTime StartAt,
    DateTime EndAt,
    string? BannerUrl,
    string Status);

public record FlashSaleItemResponse(
    Guid Id,
    Guid CampaignId,
    Guid SkuId,
    Guid SpuId,
    string SpuName,
    string SkuCode,
    string ThumbnailUrl,
    decimal OriginalPrice,
    decimal FlashPrice,
    int Quantity,
    int SoldQty,
    int ReservedQty,
    int PerUserLimit,
    string Status);
