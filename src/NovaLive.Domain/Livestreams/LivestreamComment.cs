using NovaLive.Domain.Common;

namespace NovaLive.Domain.Livestreams;

public sealed class LivestreamComment : Entity
{
    private LivestreamComment() { }

    public LivestreamComment(
        Guid sessionId,
        Guid? userId,
        string displayName,
        string content,
        bool isQuestion = false,
        DateTimeOffset? createdAt = null,
        Guid id = default)
        : base(id != default ? id : Guid.NewGuid())
    {
        SessionId = sessionId;
        UserId = userId;
        DisplayName = displayName;
        Content = content;
        IsQuestion = isQuestion;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
    }

    public Guid SessionId { get; private set; }

    public Guid? UserId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public bool IsQuestion { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
