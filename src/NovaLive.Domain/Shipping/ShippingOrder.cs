using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shipping;

public sealed class ShippingOrder : AuditableEntity
{
    public Guid SubOrderId { get; private set; }

    public ShippingProvider Provider { get; private set; }

    public string ServiceCode { get; private set; } = string.Empty;

    public string? TrackingCode { get; private set; }

    public string? ProviderOrderId { get; private set; }

    public string PickupAddressJson { get; private set; } = "{}";

    public string DeliveryAddressJson { get; private set; } = "{}";

    public decimal CodAmount { get; private set; }

    public decimal ShippingFee { get; private set; }

    public int WeightGram { get; private set; }

    public ShippingStatus Status { get; private set; } = ShippingStatus.ReadyToPick;

    public string? WebhookPayload { get; private set; }

    public DateTimeOffset? EstimatedAt { get; private set; }

    public DateTimeOffset? PickedAt { get; private set; }

    public DateTimeOffset? DeliveredAt { get; private set; }
}
