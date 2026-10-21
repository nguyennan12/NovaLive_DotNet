using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Reviews.Commands.HideReview;

public sealed record HideReviewCommand(Guid ReviewId, HideReviewRequest Request)
    : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Reviews.HideAll;
}
