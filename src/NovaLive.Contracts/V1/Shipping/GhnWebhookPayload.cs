namespace NovaLive.Contracts.V1.Shipping;

public record GhnWebhookPayload(
    string OrderCode,
    string Status,
    string TrackingCode,
    DateTime UpdatedAt,
    string Signature);
