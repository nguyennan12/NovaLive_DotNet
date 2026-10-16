namespace NovaLive.Contracts.V1.Shipping;

public record ShippingFeeResponse(
    string Provider,
    string ServiceName,
    decimal ShippingFee,
    DateTime EstimatedDeliveryDate);
