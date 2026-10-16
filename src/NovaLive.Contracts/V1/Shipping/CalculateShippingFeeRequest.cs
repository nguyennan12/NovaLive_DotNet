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
