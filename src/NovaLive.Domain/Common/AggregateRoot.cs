namespace NovaLive.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>
{
}

public abstract class AggregateRoot : Entity, IAggregateRoot<Guid>
{
    protected AggregateRoot()
    {
    }

    protected AggregateRoot(Guid id) : base(id)
    {
    }
}
