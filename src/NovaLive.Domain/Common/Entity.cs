namespace NovaLive.Domain.Common;

public abstract class Entity<TId> : IEntity<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TId Id { get; set; } = default!;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public abstract class Entity : Entity<Guid>
{
    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    protected Entity(Guid id)
    {
        Id = id;
    }
}
