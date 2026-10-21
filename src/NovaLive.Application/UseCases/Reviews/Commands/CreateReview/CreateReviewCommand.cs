using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Reviews.Commands.CreateReview;

public sealed record CreateReviewCommand(CreateReviewRequest Request)
    : ICommand<ReviewResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Reviews.CreateOwn;
}
