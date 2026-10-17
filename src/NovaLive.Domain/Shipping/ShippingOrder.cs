using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shipping;

public sealed class ShippingOrder : AuditableEntity
{
    private ShippingOrder()
    {
    }

    public ShippingOrder(
        Guid subOrderId,
        ShippingProvider provider,
        string serviceCode,
        string pickupAddressJson,
        string deliveryAddressJson,
        decimal codAmount,
        decimal shippingFee,
        int weightGram,
        string? trackingCode = null,
        string? providerOrderId = null,
        DateTimeOffset? createdAt = null,
        DateTime? estimatedAt = null)
    {
        SubOrderId = subOrderId;
        Provider = provider;
        ServiceCode = serviceCode;
        PickupAddressJson = pickupAddressJson;
        DeliveryAddressJson = deliveryAddressJson;
        CodAmount = codAmount;
        ShippingFee = shippingFee;
        WeightGram = weightGram;
        TrackingCode = trackingCode;
        ProviderOrderId = providerOrderId;
        Status = ShippingStatus.ReadyToPick;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
        EstimatedAt = estimatedAt.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(estimatedAt.Value.ToUniversalTime(), DateTimeKind.Utc))
            : null;
    }

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

    public bool UpdateStatusFromWebhook(
        ShippingStatus newStatus,
        string? webhookPayload,
        DateTimeOffset now)
    {
        if (!Enum.IsDefined(newStatus) || Status == newStatus
            || Status is ShippingStatus.Returned or ShippingStatus.Cancelled)
            return false;

        // Carrier exception states advance shipment tracking only, including returns after delivery.
        var advances = newStatus switch
        {
            ShippingStatus.Picking => Status == ShippingStatus.ReadyToPick,
            ShippingStatus.Delivering => Status is ShippingStatus.ReadyToPick or ShippingStatus.Picking,
            ShippingStatus.Delivered => Status is ShippingStatus.ReadyToPick or ShippingStatus.Picking or ShippingStatus.Delivering,
            ShippingStatus.Failed => Status is ShippingStatus.ReadyToPick or ShippingStatus.Picking
                or ShippingStatus.Delivering or ShippingStatus.Delivered,
            ShippingStatus.Returned or ShippingStatus.Cancelled => true,
            _ => false
        };
        if (!advances) return false;

        Status = newStatus;
        UpdatedAt = now;
        WebhookPayload = webhookPayload;

        if (newStatus == ShippingStatus.Picking && PickedAt is null)
        {
            PickedAt = now;
        }
        else if (newStatus == ShippingStatus.Delivered && DeliveredAt is null)
        {
            DeliveredAt = now;
        }
        return true;
    }

    public void SetTracking(string trackingCode, string? providerOrderId = null)
    {
        TrackingCode = trackingCode;
        ProviderOrderId = providerOrderId ?? trackingCode;
    }
}
