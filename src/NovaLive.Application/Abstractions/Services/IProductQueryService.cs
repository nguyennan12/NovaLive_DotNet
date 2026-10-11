using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;

namespace NovaLive.Application.Abstractions.Services;

public interface IProductQueryService
{
    Task<PagedResult<ProductSummaryResponse>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default);
}
