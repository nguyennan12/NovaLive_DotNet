namespace NovaLive.Contracts.V1.Shipping;

public sealed record TrackingEventResponse(string Status, DateTimeOffset OccurredAt, string? Description = null);
