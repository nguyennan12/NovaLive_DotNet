namespace NovaLive.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
