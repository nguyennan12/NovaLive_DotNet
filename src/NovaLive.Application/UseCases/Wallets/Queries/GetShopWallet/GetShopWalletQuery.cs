using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Wallets.Queries.GetShopWallet;

[AuthorizeRole("Seller")]
public sealed record GetShopWalletQuery : IQuery<ShopWalletResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Wallets.ViewOwn;
}
