namespace NovaLive.Contracts.V1.Shipping;

public sealed record ShipmentCreatedEvent(
    Guid ShipmentId, Guid SubOrderId, Guid ShopId, string Provider, string TrackingCode,
    decimal ShippingFee, DateTimeOffset CreatedAt);
