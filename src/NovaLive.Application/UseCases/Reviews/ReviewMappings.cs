using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Products;
using NovaLive.Domain.Reviews;
using NovaLive.Domain.Users;

namespace NovaLive.Application.UseCases.Reviews;

public static class ReviewMappings
{
    public static ReviewResponse ToResponse(this Review review, Sku? sku, User? buyer) => new(
        review.Id, review.BuyerId, MaskBuyerName(buyer?.FullName), null,
        sku?.SpuId ?? Guid.Empty, review.SkuId, sku?.AttributesJson ?? "{}",
        review.Rating, review.Title, review.Content, review.SellerReply,
        review.SellerRepliedAt?.UtcDateTime, review.CreatedAt.UtcDateTime,
        review.Images.OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).Select(i => i.MediaUrl).ToList());

    private static string MaskBuyerName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "N***";
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // Reveal initials only, including single-word names that could be an email/phone.
        return string.Join(" ", words.Select(word => char.IsLetter(word[0]) ? $"{word[0]}***" : "***"));
    }
}
