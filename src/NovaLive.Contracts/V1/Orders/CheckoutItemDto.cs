namespace NovaLive.Contracts.V1.Orders;

public record CheckoutItemDto(
    Guid SkuId,
    string SpuName,
    string SkuCode,
    string AttributesJson,
    string ThumbnailUrl,
    decimal Price,
    int Quantity,
    decimal LineTotal);
