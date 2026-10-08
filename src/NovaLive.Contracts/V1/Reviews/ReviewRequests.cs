namespace NovaLive.Contracts.V1.Reviews;

public record CreateReviewRequest(
    Guid OrderItemId,
    int Rating,
    string? Title,
    string? Content,
    List<string>? MediaUrls);

public record UpdateReviewRequest(
    int Rating,
    string? Title,
    string? Content,
    List<string>? MediaUrls);

public record SellerReplyReviewRequest(
    string ReplyText);
