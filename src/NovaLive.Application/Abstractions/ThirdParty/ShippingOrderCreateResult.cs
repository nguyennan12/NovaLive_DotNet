namespace NovaLive.Application.Abstractions.ThirdParty;

public sealed record ShippingOrderCreateResult(
    string ProviderOrderCode,
    string TrackingCode,
    decimal TotalFee,
    DateTime? ExpectedDeliveryDate = null);
