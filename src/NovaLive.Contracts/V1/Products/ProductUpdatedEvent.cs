namespace NovaLive.Contracts.V1.Products;

// Matches the ProductUpdatedIntegrationEvent payload already persisted by T16.
public sealed record ProductUpdatedEvent(
    Guid SpuId,
    Guid ShopId,
    string Action,
    Guid? SkuId = null,
    DateTimeOffset OccurredAt = default);
