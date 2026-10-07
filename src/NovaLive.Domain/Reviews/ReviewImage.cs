using NovaLive.Domain.Common;

namespace NovaLive.Domain.Reviews;

public sealed class ReviewImage : Entity
{
    public Guid ReviewId { get; set; }

    public string MediaUrl { get; set; } = string.Empty;

    public ReviewMediaType MediaType { get; set; } = ReviewMediaType.image;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
