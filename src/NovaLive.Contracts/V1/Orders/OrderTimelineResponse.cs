namespace NovaLive.Contracts.V1.Orders;
public sealed record OrderTimelineResponse(string? FromStatus, string ToStatus, string? ChangedByRole, string? Note, DateTimeOffset ChangedAt);
