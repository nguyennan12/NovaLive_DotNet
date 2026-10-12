using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Products.Commands.UpdateSpu;

public record UpdateSpuCommand(Guid SpuId, UpdateSpuRequest Request) 
    : ICommand<SpuDetailResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Products.UpdateOwn;
}
