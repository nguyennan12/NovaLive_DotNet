using NovaLive.Domain.Common;

namespace NovaLive.Domain.Livestreams;

public sealed class LivestreamComment : Entity
{
    public Guid SessionId { get; set; }

    public Guid? UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public bool IsQuestion { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
