using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Carts.Commands.RemoveCartItem;
[RequireAuthenticated]
public sealed record RemoveCartItemCommand(Guid CartItemId) : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Carts.ManageOwn;
}
