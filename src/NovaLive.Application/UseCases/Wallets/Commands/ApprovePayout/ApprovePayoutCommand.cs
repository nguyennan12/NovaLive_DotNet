using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Wallets.Commands.ApprovePayout;

[AuthorizeRole("Admin")]
public sealed record ApprovePayoutCommand(Guid PayoutId, ApprovePayoutRequest Request)
    : ICommand<SellerPayoutResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Wallets.ApprovePayout;
}
