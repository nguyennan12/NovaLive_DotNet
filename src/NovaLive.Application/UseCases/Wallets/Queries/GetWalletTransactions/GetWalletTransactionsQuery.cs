using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Wallets.Queries.GetWalletTransactions;

[AuthorizeRole("Seller")]
public sealed record GetWalletTransactionsQuery(GetWalletTransactionsRequest Request)
    : IQuery<PagedResult<WalletTransactionResponse>>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Wallets.ViewOwn;
}
