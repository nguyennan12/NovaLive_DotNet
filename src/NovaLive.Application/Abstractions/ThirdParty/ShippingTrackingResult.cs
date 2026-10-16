using NovaLive.Contracts.V1.Shipping;

namespace NovaLive.Application.Abstractions.ThirdParty;

public sealed record ShippingTrackingResult(string Status, IReadOnlyList<TrackingEventResponse> Events);
