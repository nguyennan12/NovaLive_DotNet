using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products.Events;

public sealed record ProductUpdatedDomainEvent(Guid SpuId, Guid ShopId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
