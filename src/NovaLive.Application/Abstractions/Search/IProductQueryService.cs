using NovaLive.Contracts.Products;

namespace NovaLive.Application.Abstractions.Search;

public interface IProductQueryService
{
    Task<IReadOnlyList<ProductSummaryDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default);
}
