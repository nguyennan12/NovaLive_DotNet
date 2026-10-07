using NovaLive.Domain.Common;

namespace NovaLive.Domain.System;

public sealed class OutboxMessage : Entity
{
    private OutboxMessage() { }

    public OutboxMessage(string eventType, string payload)
    {
        EventType = eventType;
        Payload = payload;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string EventType { get; private set; } = string.Empty;

    public string Payload { get; private set; } = "{}";

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ProcessedAt { get; private set; }

    public int RetryCount { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        RetryCount++;
        ErrorMessage = errorMessage;
    }
}
