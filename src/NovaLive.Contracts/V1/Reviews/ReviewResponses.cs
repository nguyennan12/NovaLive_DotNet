namespace NovaLive.Contracts.V1.Reviews;

public record ReviewResponse(
    Guid Id,
    Guid BuyerId,
    string BuyerName,
    string? BuyerAvatarUrl,
    Guid SpuId,
    Guid SkuId,
    string SkuAttributesJson,
    int Rating,
    string? Title,
    string? Content,
    string? SellerReply,
    DateTime? RepliedAt,
    DateTime CreatedAt,
    List<string> MediaUrls);
