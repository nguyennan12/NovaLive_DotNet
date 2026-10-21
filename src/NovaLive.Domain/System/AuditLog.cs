using NovaLive.Domain.Common;

namespace NovaLive.Domain.System;

public sealed class AuditLog : Entity
{
    private AuditLog() { }

    public static AuditLog Create(Guid userId, string action, string entityType, Guid entityId,
        string oldDataJson, string newDataJson, DateTimeOffset now) => new()
        {
            UserId = userId, Action = action, EntityType = entityType, EntityId = entityId,
            OldDataJson = oldDataJson, NewDataJson = newDataJson, CreatedAt = now
        };

    public Guid? UserId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public Guid? EntityId { get; private set; }

    public string? OldDataJson { get; private set; }

    public string? NewDataJson { get; private set; }

    public string? IpAddress { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
