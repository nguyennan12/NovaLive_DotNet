namespace NovaLive.Contracts.V1.FlashSales;

public record CreateFlashSaleCampaignRequest(
    string Name,
    DateTime StartAt,
    DateTime EndAt,
    string? BannerUrl);

public record RegisterFlashSaleItemRequest(
    Guid SkuId,
    decimal FlashPrice,
    int Quantity,
    int PerUserLimit);

public record RejectFlashSaleItemRequest(
    string Reason);
