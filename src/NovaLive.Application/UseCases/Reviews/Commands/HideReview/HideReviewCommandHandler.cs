using System.Text.Json;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Domain.Reviews;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Reviews.Commands.HideReview;

public sealed class HideReviewCommandHandler(
    IReviewRepository reviews, IAuditLogRepository audit, ICurrentUser user, IDateTimeProvider clock)
    : ICommandHandler<HideReviewCommand>
{
    public async Task<Result> Handle(HideReviewCommand command, CancellationToken ct)
    {
        if (user.UserId is not { } admin || admin == Guid.Empty) return ReviewErrors.NotFound;
        var review = await reviews.GetForUpdateAsync(command.ReviewId, ct);
        if (review is null) return ReviewErrors.NotFound;
        var wasVisible = review.IsVisible;
        var now = clock.UtcNow;
        var reason = command.Request.Reason;
        var result = review.Hide(reason, now);
        if (result.IsFailure) return result.Error;
        if (!wasVisible) return Result.Success();

        await audit.AddAsync(AuditLog.Create(admin, "Review.Hidden", "Review", review.Id,
            JsonSerializer.Serialize(new { is_visible = wasVisible, reason }),
            JsonSerializer.Serialize(new { is_visible = review.IsVisible, reason }), now), ct);
        return Result.Success();
    }
}
