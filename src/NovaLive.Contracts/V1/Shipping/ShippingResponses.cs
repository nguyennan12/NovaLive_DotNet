namespace NovaLive.Contracts.V1.Shipping;

public record ShippingFeeResponse(
    string Provider,
    string ServiceName,
    decimal ShippingFee,
    DateTime EstimatedDeliveryDate);

public record ShippingOrderResponse(
    Guid Id,
    Guid SubOrderId,
    string Provider,
    string TrackingCode,
    decimal ShippingFee,
    string Status,
    DateTime CreatedAt);
