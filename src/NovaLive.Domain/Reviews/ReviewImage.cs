using NovaLive.Domain.Common;

namespace NovaLive.Domain.Reviews;

public sealed class ReviewImage : Entity
{
    public Guid ReviewId { get; private set; }

    public string MediaUrl { get; private set; } = string.Empty;

    public ReviewMediaType MediaType { get; private set; } = ReviewMediaType.image;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
