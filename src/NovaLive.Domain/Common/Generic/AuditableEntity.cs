namespace NovaLive.Domain.Common;

public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public Guid? CreatedBy { get; protected set; }
    public DateTimeOffset UpdatedAt { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }

    // Persistence stamps metadata through the interface while domain consumers only read it.
    DateTimeOffset IAuditableEntity.CreatedAt { get => CreatedAt; set => CreatedAt = value; }
    Guid? IAuditableEntity.CreatedBy { get => CreatedBy; set => CreatedBy = value; }
    DateTimeOffset IAuditableEntity.UpdatedAt { get => UpdatedAt; set => UpdatedAt = value; }
    Guid? IAuditableEntity.UpdatedBy { get => UpdatedBy; set => UpdatedBy = value; }
}
