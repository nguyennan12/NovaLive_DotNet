namespace NovaLive.Contracts.V1.Shipping;

public sealed record ShipmentDeliveredEvent(
    Guid ShippingOrderId,
    Guid SubOrderId,
    Guid ParentOrderId,
    Guid ShopId,
    string Provider,
    string TrackingCode,
    DateTimeOffset DeliveredAt,
    bool IsVerifiedWebhook = false);
