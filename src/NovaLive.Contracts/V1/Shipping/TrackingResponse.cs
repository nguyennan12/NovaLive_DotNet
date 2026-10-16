namespace NovaLive.Contracts.V1.Shipping;

public sealed record TrackingResponse(
    Guid SubOrderId,
    string Provider,
    string TrackingCode,
    string Status,
    IReadOnlyList<TrackingEventResponse> Events,
    bool IsStale);
