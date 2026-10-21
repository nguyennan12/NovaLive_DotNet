using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Wallets.Commands.CreatePayoutRequest;

[AuthorizeRole("Seller")]
public sealed record CreatePayoutRequestCommand(NovaLive.Contracts.V1.Shops.CreatePayoutRequest Request)
    : ICommand<SellerPayoutResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Wallets.RequestPayout;
}
