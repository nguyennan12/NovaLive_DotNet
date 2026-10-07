using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shipping;

public sealed class ShippingOrder : AuditableEntity
{
    public Guid SubOrderId { get; set; }

    public ShippingProvider Provider { get; set; }

    public string ServiceCode { get; set; } = string.Empty;

    public string? TrackingCode { get; set; }

    public string? ProviderOrderId { get; set; }

    public string PickupAddressJson { get; set; } = "{}";

    public string DeliveryAddressJson { get; set; } = "{}";

    public decimal CodAmount { get; set; }

    public decimal ShippingFee { get; set; }

    public int WeightGram { get; set; }

    public ShippingStatus Status { get; set; } = ShippingStatus.ReadyToPick;

    public string? WebhookPayload { get; set; }

    public DateTimeOffset? EstimatedAt { get; set; }

    public DateTimeOffset? PickedAt { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }
}
