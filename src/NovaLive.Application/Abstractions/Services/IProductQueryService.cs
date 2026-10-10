using NovaLive.Contracts.V1.Products;

namespace NovaLive.Application.Abstractions.Services;

public interface IProductQueryService
{
    Task<IReadOnlyList<ProductSummaryDto>> SearchAsync(SearchProductsRequest request, CancellationToken cancellationToken = default);
}
