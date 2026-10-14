using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Carts.Commands.UpdateCartItem;
[RequireAuthenticated]
public sealed record UpdateCartItemCommand(Guid CartItemId, UpdateCartItemRequest Request) : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Carts.ManageOwn;
}
