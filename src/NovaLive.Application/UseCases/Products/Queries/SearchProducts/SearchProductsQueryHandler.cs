using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Products.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler(
    IProductQueryService queryService,
    ICacheService cacheService)
    : IQueryHandler<SearchProductsQuery, PagedResult<ProductSummaryResponse>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    public async Task<Result<PagedResult<ProductSummaryResponse>>> Handle(
        SearchProductsQuery query,
        CancellationToken ct)
    {
        var req = query.Request;
        var cacheKey = $"products:search:kw:{req.Keyword?.Trim().ToLowerInvariant()}:cat:{req.CategoryId}:shop:{req.ShopId}:min:{req.MinPrice}:max:{req.MaxPrice}:sort:{req.SortBy?.Trim().ToLowerInvariant()}:p:{req.Page}:s:{req.Size}";

        var cached = await cacheService.GetAsync<PagedResult<ProductSummaryResponse>>(cacheKey, ct);
        if (cached is not null)
        {
            return cached;
        }

        var result = await queryService.SearchAsync(req, ct);

        await cacheService.SetAsync(cacheKey, result, CacheTtl, ct);

        return result;
    }
}
