using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Wallets.Commands.RejectPayout;

[AuthorizeRole("Admin")]
public sealed record RejectPayoutCommand(Guid PayoutId, RejectPayoutRequest Request)
    : ICommand<SellerPayoutResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Wallets.ApprovePayout;
}
