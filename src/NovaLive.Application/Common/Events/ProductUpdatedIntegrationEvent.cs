namespace NovaLive.Application.Common.Events;

public sealed record ProductUpdatedIntegrationEvent(
    Guid SpuId,
    Guid ShopId,
    string Action,
    Guid? SkuId = null,
    DateTimeOffset OccurredAt = default)
{
    public DateTimeOffset OccurredAt { get; init; } = OccurredAt == default ? DateTimeOffset.UtcNow : OccurredAt;
}
