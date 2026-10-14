namespace NovaLive.Contracts.V1.FlashSales;

public record CreateFlashSaleCampaignRequest(
    string Name,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? BannerUrl);

public record RegisterFlashSaleSkuRequest(
    Guid SkuId,
    decimal FlashPrice,
    int Quantity,
    int PerUserLimit);

public record RegisterFlashSaleItemRequest(
    Guid SkuId,
    decimal FlashPrice,
    int Quantity,
    int PerUserLimit) : RegisterFlashSaleSkuRequest(SkuId, FlashPrice, Quantity, PerUserLimit);

public record RejectFlashSaleItemRequest(
    string Reason);
