using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Reviews.Commands.UpdateReview;

public sealed record UpdateReviewCommand(Guid ReviewId, UpdateReviewRequest Request)
    : ICommand<ReviewResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Reviews.UpdateOwn;
}
