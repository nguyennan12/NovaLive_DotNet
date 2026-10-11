using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;

namespace NovaLive.Application.UseCases.Products.Queries.SearchProducts;

public sealed record SearchProductsQuery(ProductSearchRequest Request)
    : IQuery<PagedResult<ProductSummaryResponse>>;
