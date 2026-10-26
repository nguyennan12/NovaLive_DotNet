using NovaLive.Domain.Common;

namespace NovaLive.Domain.Notifications;

public sealed class Notification : Entity
{
    private Notification() { }

    public Notification(Guid userId, string type, string title, string body, string? refType, Guid? refId)
    {
        UserId = userId;
        Type = type;
        Title = title;
        Body = body;
        RefType = refType;
        RefId = refId;
    }

    public Guid UserId { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public string? RefType { get; private set; }

    public Guid? RefId { get; private set; }

    public bool IsRead { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public void MarkAsRead(DateTimeOffset now)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAt = now;
    }
}
