namespace NovaLive.Application.Abstractions.ThirdParty;

public sealed record ShippingFeeQuote(
    string ProviderCode,
    string ServiceCode,
    decimal TotalFee,
    DateTime? ExpectedDeliveryDate = null);
