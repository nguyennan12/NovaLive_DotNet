using NovaLive.Domain.Common;

namespace NovaLive.Domain.Notifications;

public sealed class Notification : Entity
{
    public Guid UserId { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public string? RefType { get; private set; }

    public Guid? RefId { get; private set; }

    public bool IsRead { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
