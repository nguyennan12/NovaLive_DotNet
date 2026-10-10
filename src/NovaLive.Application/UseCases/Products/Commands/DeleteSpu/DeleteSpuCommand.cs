using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Products.Commands.DeleteSpu;

public record DeleteSpuCommand(Guid SpuId) 
    : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Products.DeleteOwn;
}
