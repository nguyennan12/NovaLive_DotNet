using NovaLive.Domain.Common;

namespace NovaLive.Domain.Reviews;

public sealed class ReviewImage : Entity
{
    private ReviewImage() { }

    internal static ReviewImage Create(Guid reviewId, string mediaUrl, ReviewMediaType mediaType, DateTimeOffset now) =>
        new() { ReviewId = reviewId, MediaUrl = mediaUrl, MediaType = mediaType, CreatedAt = now };

    public Guid ReviewId { get; private set; }

    public string MediaUrl { get; private set; } = string.Empty;

    public ReviewMediaType MediaType { get; private set; } = ReviewMediaType.image;

    public DateTimeOffset CreatedAt { get; private set; }
}
