using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Products.Queries.GetSpuDetail;

public record GetSpuDetailQuery(Guid SpuId) : IQuery<SpuDetailResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Products.ViewOwn;
}
