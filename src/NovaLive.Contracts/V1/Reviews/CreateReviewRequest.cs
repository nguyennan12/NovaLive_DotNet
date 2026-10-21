namespace NovaLive.Contracts.V1.Reviews;

public record CreateReviewRequest(
    Guid OrderItemId,
    int Rating,
    string? Title,
    string? Content,
    List<string>? MediaUrls);
