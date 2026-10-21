using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Common;
using NovaLive.Domain.Reviews;

namespace NovaLive.Application.UseCases.Reviews.Commands.UpdateReview;

public sealed class UpdateReviewCommandHandler(
    IReviewRepository reviews, ICurrentUser user, IDateTimeProvider clock, ReviewResponseReader reader)
    : ICommandHandler<UpdateReviewCommand, ReviewResponse>
{
    public async Task<Result<ReviewResponse>> Handle(UpdateReviewCommand command, CancellationToken ct)
    {
        var review = await reviews.GetForUpdateAsync(command.ReviewId, ct);
        if (review is null || review.BuyerId != user.UserId) return ReviewErrors.NotFound;
        var request = command.Request;
        var result = review.Update(request.Rating, request.Title, request.Content, request.MediaUrls, clock.UtcNow);
        if (result.IsFailure) return result.Error;
        reviews.ReplaceImages(review);
        return await reader.ReadAsync(review, ct);
    }
}
