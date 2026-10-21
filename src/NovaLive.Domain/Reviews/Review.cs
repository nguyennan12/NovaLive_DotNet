using NovaLive.Domain.Common;

namespace NovaLive.Domain.Reviews;

public sealed class Review : AuditableEntity<Guid>
{
    private readonly List<ReviewImage> _images = [];
    private bool _imagesRestored;
    private Review() { Id = Guid.NewGuid(); }

    public IReadOnlyCollection<ReviewImage> Images => _images.AsReadOnly();

    // Persistence restores the collection once after materializing an existing review.
    // Newly created/edited aggregates cannot use hydration to bypass the edit-once rule.
    public Result RestoreImages(IReadOnlyCollection<ReviewImage> images)
    {
        if (_imagesRestored || images.Any(image => image.ReviewId != Id)) return ReviewErrors.InvalidMedia;
        _images.AddRange(images);
        _imagesRestored = true;
        return Result.Success();
    }

    public static Result ValidatePurchase(SubOrderStatus status, OrderItemStatus itemStatus) =>
        status is SubOrderStatus.Delivered or SubOrderStatus.Completed && itemStatus != OrderItemStatus.Returned
            ? Result.Success() : ReviewErrors.NotDelivered;

    public static Result<Review> Create(Guid orderItemId, Guid skuId, Guid shopId, Guid buyerId,
        int rating, string? title, string? content, IReadOnlyCollection<string>? mediaUrls, DateTimeOffset now)
    {
        if (orderItemId == Guid.Empty || skuId == Guid.Empty || shopId == Guid.Empty || buyerId == Guid.Empty)
            return ReviewErrors.InvalidIdentity;
        var validation = ValidateContent(rating, title, content, mediaUrls);
        if (validation.IsFailure) return validation.Error;
        var review = new Review
        {
            OrderItemId = orderItemId, SkuId = skuId, ShopId = shopId, BuyerId = buyerId,
            Rating = (short)rating, Title = title, Content = content, EditCount = 0,
            IsVisible = true, CreatedAt = now, UpdatedAt = now
        };
        review.ReplaceImages(mediaUrls, now);
        return review;
    }

    public Result Update(int rating, string? title, string? content,
        IReadOnlyCollection<string>? mediaUrls, DateTimeOffset now)
    {
        if (!IsVisible) return ReviewErrors.Hidden;
        if (EditCount != 0) return ReviewErrors.AlreadyEdited;
        if (now > CreatedAt.AddDays(ReviewLimits.EditWindowDays)) return ReviewErrors.EditExpired;
        var validation = ValidateContent(rating, title, content, mediaUrls);
        if (validation.IsFailure) return validation.Error;
        Rating = (short)rating;
        Title = title;
        Content = content;
        EditCount++;
        ReplaceImages(mediaUrls, now);
        UpdatedAt = now;
        return Result.Success();
    }

    public Result Reply(string replyText, DateTimeOffset now)
    {
        if (SellerReply is not null) return ReviewErrors.AlreadyReplied;
        if (string.IsNullOrWhiteSpace(replyText) || replyText.Length > ReviewLimits.ReplyLength)
            return ReviewErrors.InvalidReply;
        SellerReply = replyText;
        SellerRepliedAt = now;
        UpdatedAt = now;
        return Result.Success();
    }

    public Result Hide(string reason, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > ReviewLimits.ReasonLength)
            return ReviewErrors.InvalidReason;
        if (!IsVisible) return Result.Success();
        IsVisible = false;
        UpdatedAt = now;
        return Result.Success();
    }

    private static Result ValidateContent(int rating, string? title, string? content,
        IReadOnlyCollection<string>? mediaUrls)
    {
        if (rating is < ReviewLimits.MinRating or > ReviewLimits.MaxRating) return ReviewErrors.InvalidRating;
        if (title?.Length > ReviewLimits.TitleLength || content?.Length > ReviewLimits.ContentLength)
            return ReviewErrors.InvalidText;
        if (mediaUrls?.Count > ReviewLimits.MediaCount || mediaUrls?.Any(url => !ReviewMedia.IsValid(url)) == true)
            return ReviewErrors.InvalidMedia;
        return Result.Success();
    }

    private void ReplaceImages(IReadOnlyCollection<string>? urls, DateTimeOffset now)
    {
        _imagesRestored = true;
        _images.Clear();
        foreach (var url in urls ?? [])
            _images.Add(ReviewImage.Create(Id, url, ReviewMedia.TryGetType(url).Value, now));
    }

    public Guid OrderItemId { get; private set; }

    public Guid SkuId { get; private set; }

    public Guid ShopId { get; private set; }

    public Guid BuyerId { get; private set; }

    public short Rating { get; private set; }

    public string? Title { get; private set; }

    public string? Content { get; private set; }

    public string? SellerReply { get; private set; }

    public DateTimeOffset? SellerRepliedAt { get; private set; }

    public short EditCount { get; private set; }

    public bool IsVisible { get; private set; } = true;
}
