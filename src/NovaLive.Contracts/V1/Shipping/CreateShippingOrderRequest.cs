namespace NovaLive.Contracts.V1.Shipping;

public record CreateShippingOrderRequest(
    Guid SubOrderId,
    string Provider,
    string ServiceCode,
    Guid PickupAddressId);
