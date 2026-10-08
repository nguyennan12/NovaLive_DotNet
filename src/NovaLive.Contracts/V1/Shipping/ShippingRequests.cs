namespace NovaLive.Contracts.V1.Shipping;

public record CalculateShippingFeeRequest(
    Guid ShopId,
    Guid WarehouseAddressId,
    Guid ShippingAddressId,
    List<ShippingItemInputDto> Items);

public record ShippingItemInputDto(
    Guid SkuId,
    int Quantity,
    int WeightGram);

public record CreateShippingOrderRequest(
    Guid SubOrderId,
    string Provider,
    string ServiceCode,
    Guid PickupAddressId);

public record GhnWebhookPayload(
    string OrderCode,
    string Status,
    string TrackingCode,
    DateTime UpdatedAt,
    string Signature);
