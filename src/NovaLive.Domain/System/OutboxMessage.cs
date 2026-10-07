using NovaLive.Domain.Common;

namespace NovaLive.Domain.System;

public sealed class OutboxMessage : Entity
{
    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ProcessedAt { get; set; }

    public int RetryCount { get; set; }

    public string? ErrorMessage { get; set; }
}
