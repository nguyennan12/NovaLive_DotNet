namespace NovaLive.Contracts.V1.Reviews;

public record UpdateReviewRequest(
    int Rating,
    string? Title,
    string? Content,
    List<string>? MediaUrls);
