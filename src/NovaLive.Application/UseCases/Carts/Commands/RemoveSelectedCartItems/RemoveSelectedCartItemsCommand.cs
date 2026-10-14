using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Carts.Commands.RemoveSelectedCartItems;
[RequireAuthenticated]
public sealed record RemoveSelectedCartItemsCommand(RemoveCartItemsRequest Request) : ICommand<int>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Carts.ManageOwn;
}
