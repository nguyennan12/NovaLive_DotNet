namespace NovaLive.Contracts.V1.Shipping;

public record ShippingItemInputDto(
    Guid SkuId,
    int Quantity,
    int WeightGram);
