using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Products.Commands.UpdateSkuPrice;

public record UpdateSkuPriceCommand(Guid SkuId, UpdateSkuPriceRequest Request) 
    : ICommand<SkuResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Products.UpdateOwn;
}
