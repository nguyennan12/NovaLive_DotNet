using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Carts.Queries.GetCart;
[RequireAuthenticated]
public sealed record GetCartQuery() : IQuery<CartResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Carts.ManageOwn;
}
