namespace NovaLive.Contracts.V1.Orders;

public record OrderItemResponse(
    Guid Id,
    Guid SkuId,
    string SpuName,
    string SkuCode,
    string AttributesJson,
    string ThumbnailUrl,
    decimal Price,
    int Quantity,
    decimal LineTotal);
