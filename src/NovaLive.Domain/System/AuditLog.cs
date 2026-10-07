using NovaLive.Domain.Common;

namespace NovaLive.Domain.System;

public sealed class AuditLog : Entity
{
    public Guid? UserId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public Guid? EntityId { get; private set; }

    public string? OldDataJson { get; private set; }

    public string? NewDataJson { get; private set; }

    public string? IpAddress { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
