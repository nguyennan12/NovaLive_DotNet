namespace NovaLive.Domain.Common;

public interface IAggregateRoot : IEntity
{
}

public interface IAggregateRoot<out TId> : IEntity<TId>, IAggregateRoot
{
}
