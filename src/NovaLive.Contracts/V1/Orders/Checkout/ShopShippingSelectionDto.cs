namespace NovaLive.Contracts.V1.Orders;

public record ShopShippingSelectionDto(
    Guid ShopId,
    string Provider,
    string ServiceCode);
