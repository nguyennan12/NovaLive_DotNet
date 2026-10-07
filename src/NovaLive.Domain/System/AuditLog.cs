using NovaLive.Domain.Common;

namespace NovaLive.Domain.System;

public sealed class AuditLog : Entity
{
    public Guid? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public Guid? EntityId { get; set; }

    public string? OldDataJson { get; set; }

    public string? NewDataJson { get; set; }

    public string? IpAddress { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
