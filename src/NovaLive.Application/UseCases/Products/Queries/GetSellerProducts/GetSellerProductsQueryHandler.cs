using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Products.Queries.GetSellerProducts;

public sealed class GetSellerProductsQueryHandler(
    ISpuRepository spuRepository,
    ISkuRepository skuRepository,
    ICategoryRepository categoryRepository,
    IAppDbContext dbContext,
    ICurrentUser currentUser)
    : IQueryHandler<GetSellerProductsQuery, PagedResult<SpuResponse>>
{
    public async Task<Result<PagedResult<SpuResponse>>> Handle(GetSellerProductsQuery query, CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return ProductErrors.UnauthorizedShop;
        }

        var req = query.Request;

        var (spus, total) = await spuRepository.GetPagedAsync(
            shopId: shopId.Value,
            categoryId: req.CategoryId,
            keyword: req.Keyword,
            page: req.NormalizedPage,
            size: req.NormalizedSize,
            ct: ct);

        var spuIds = spus.Select(s => s.Id).ToList();
        var allCategories = await categoryRepository.GetAllAsync(ct);
        var categoryMap = allCategories.ToDictionary(c => c.Id, c => c.Name);

        var skuMap = await skuRepository.GetBySpuIdsAsync(spuIds, ct);

        var shop = await dbContext.Shops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == shopId.Value, ct);

        var shopName = shop?.ShopName ?? "Gian hàng";

        var items = spus.Select(spu =>
        {
            skuMap.TryGetValue(spu.Id, out var spuSkus);
            var categoryName = categoryMap.TryGetValue(spu.CategoryId, out var cName) ? cName : "Danh mục";
            return spu.ToSummaryResponse(shopName, categoryName, spuSkus ?? []);
        }).ToList();

        return PagedResult<SpuResponse>.Create(items, req.NormalizedPage, req.NormalizedSize, total);
    }
}
