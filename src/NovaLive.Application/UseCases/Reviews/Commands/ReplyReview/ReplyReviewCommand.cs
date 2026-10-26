using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Reviews.Commands.ReplyReview;

public sealed record ReplyReviewCommand(Guid ReviewId, SellerReplyReviewRequest Request)
    : ICommand<ReviewResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Reviews.ReplyOwnShop;
}
