using NovaLive.Domain.Common;

namespace NovaLive.Domain.Livestreams;

public sealed class LivestreamComment : Entity
{
    public Guid SessionId { get; private set; }

    public Guid? UserId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public bool IsQuestion { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
