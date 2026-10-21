using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Common;
using NovaLive.Domain.Reviews;

namespace NovaLive.Application.UseCases.Reviews.Commands.CreateReview;

public sealed class CreateReviewCommandHandler(
    IOrderItemRepository items, ISubOrderRepository subs, IParentOrderRepository parents,
    IReviewRepository reviews, ICurrentUser user, IDateTimeProvider clock, ReviewResponseReader reader)
    : ICommandHandler<CreateReviewCommand, ReviewResponse>
{
    public async Task<Result<ReviewResponse>> Handle(CreateReviewCommand command, CancellationToken ct)
    {
        if (user.UserId is not { } buyer || buyer == Guid.Empty) return ReviewErrors.OrderItemNotFound;
        var request = command.Request;
        var item = await items.GetByIdAsync(request.OrderItemId, ct);
        if (item is null) return ReviewErrors.OrderItemNotFound;
        var sub = await subs.FindByIdAsync(item.SubOrderId, ct);
        if (sub is null) return ReviewErrors.OrderItemNotFound;
        var parent = await parents.FindAsync(sub.ParentOrderId, buyer, ct);
        if (parent is null || parent.BuyerId != buyer) return ReviewErrors.OrderItemNotFound;

        var eligibility = Review.ValidatePurchase(sub.Status, item.Status);
        if (eligibility.IsFailure) return eligibility.Error;
        if (await reviews.ExistsByOrderItemAsync(item.Id, ct)) return ReviewErrors.AlreadyReviewed;
        var created = Review.Create(item.Id, item.SkuId, sub.ShopId, buyer,
            request.Rating, request.Title, request.Content, request.MediaUrls, clock.UtcNow);
        if (created.IsFailure) return created.Error;
        var review = created.Value!;
        // TryAdd flushes inside the command transaction and translates only the order-item UNIQUE collision.
        if (!await reviews.TryAddAsync(review, ct)) return ReviewErrors.AlreadyReviewed;
        return await reader.ReadAsync(review, ct);
    }
}
