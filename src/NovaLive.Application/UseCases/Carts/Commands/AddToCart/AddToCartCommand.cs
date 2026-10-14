using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Carts.Commands.AddToCart;
[RequireAuthenticated]
public sealed record AddToCartCommand(AddToCartRequest Request) : ICommand<CartItemDto>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Carts.ManageOwn;
}
