namespace NovaLive.Contracts.V1.Shipping;

public record CalculateShippingFeeRequest(
    Guid ShopId,
    Guid WarehouseAddressId,
    Guid ShippingAddressId,
    List<ShippingItemInputDto>? Items = null,
    int? WeightGram = null,
    int? LengthCm = null,
    int? WidthCm = null,
    int? HeightCm = null,
    decimal? InsuranceValue = null);

public record ShippingItemInputDto(
    Guid SkuId,
    int Quantity,
    int WeightGram);

public record CreateShippingOrderRequest(
    Guid SubOrderId,
    string Provider,
    string ServiceCode,
    Guid PickupAddressId);

public record GhnWebhookRequest(
    string OrderCode,
    string Status,
    string? TrackingCode = null,
    int? Weight = null,
    decimal? TotalFee = null,
    decimal? CodAmount = null,
    string? Reason = null,
    DateTime? UpdatedAt = null);

public record GhnWebhookPayload(
    string OrderCode,
    string Status,
    string TrackingCode,
    DateTime UpdatedAt,
    string Signature);

