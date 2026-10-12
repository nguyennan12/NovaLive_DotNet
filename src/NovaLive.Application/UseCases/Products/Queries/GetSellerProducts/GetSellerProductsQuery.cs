using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Products.Queries.GetSellerProducts;

public record GetSellerProductsQuery(SearchProductsRequest Request) 
    : IQuery<PagedResult<SpuResponse>>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Products.ViewOwn;
}
