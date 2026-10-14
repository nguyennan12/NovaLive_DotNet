namespace NovaLive.Application.Common.Events;

// Deliberately excludes KYC URLs and banking details.
public sealed record ShopRegisteredEvent(Guid ShopId, Guid OwnerId, DateTimeOffset OccurredAt);
