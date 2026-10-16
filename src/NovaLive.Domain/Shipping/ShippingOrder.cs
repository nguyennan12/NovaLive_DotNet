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

    public void UpdateStatusFromWebhook(
        ShippingStatus newStatus,
        string? webhookPayload,
        DateTimeOffset? eventTime = null)
    {
        WebhookPayload = webhookPayload;

        // Tránh ghi đè ngược khi đơn đã hoàn tất giao
        if (Status == ShippingStatus.Delivered && newStatus != ShippingStatus.Delivered)
        {
            return;
        }

        Status = newStatus;
        var now = eventTime ?? DateTimeOffset.UtcNow;

        if (newStatus == ShippingStatus.Picking && PickedAt is null)
        {
            PickedAt = now;
        }
        else if (newStatus == ShippingStatus.Delivered && DeliveredAt is null)
        {
            DeliveredAt = now;
        }
    }

    public void SetTracking(string trackingCode, string? providerOrderId = null)
    {
        TrackingCode = trackingCode;
        ProviderOrderId = providerOrderId ?? trackingCode;
    }
}
