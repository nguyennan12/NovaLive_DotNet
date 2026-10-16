namespace NovaLive.Contracts.V1.Shipping;

public record GhnWebhookRequest(
    string OrderCode,
    string Status,
    string? TrackingCode = null,
    int? Weight = null,
    decimal? TotalFee = null,
    decimal? CodAmount = null,
    string? Reason = null,
    DateTime? UpdatedAt = null);
