namespace NovaLive.Domain.Common;

public interface IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}

public interface IEntity<out TId> : IEntity
{
    TId Id { get; }
}
