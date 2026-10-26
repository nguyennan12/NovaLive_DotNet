using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Common;
using NovaLive.Domain.Reviews;

namespace NovaLive.Application.UseCases.Reviews.Commands.ReplyReview;

public sealed class ReplyReviewCommandHandler(
    IReviewRepository reviews, ICurrentShop shop, ICurrentUser user, IShopRepository shops,
    IDateTimeProvider clock, ReviewResponseReader reader)
    : ICommandHandler<ReplyReviewCommand, ReviewResponse>
{
    public async Task<Result<ReviewResponse>> Handle(ReplyReviewCommand command, CancellationToken ct)
    {
        if (user.UserId is not { } seller || shop.ShopId is not { } shopId) return ReviewErrors.NotFound;
        // ICurrentShop can come from X-Shop-Id: verify ownership against persisted shop data.
        var ownedShop = await shops.GetByOwnerIdAsync(seller, ct);
        if (ownedShop is null || ownedShop.Id != shopId) return ReviewErrors.NotFound;
        var review = await reviews.GetForUpdateAsync(command.ReviewId, ct);
        if (review is null || review.ShopId != shop.ShopId) return ReviewErrors.NotFound;
        var result = review.Reply(command.Request.ReplyText, clock.UtcNow);
        if (result.IsFailure) return result.Error;
        return await reader.ReadAsync(review, ct);
    }
}
