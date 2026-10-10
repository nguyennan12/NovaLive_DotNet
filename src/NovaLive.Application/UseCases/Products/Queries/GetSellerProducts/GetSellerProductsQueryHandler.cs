using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Products.Queries.GetSellerProducts;

public sealed class GetSellerProductsQueryHandler(
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
        var page = req.Page <= 0 ? 1 : req.Page;
        var size = req.Size <= 0 ? 20 : (req.Size > 100 ? 100 : req.Size);

        var spuQuery = dbContext.Spus
            .AsNoTracking()
            .Where(s => s.ShopId == shopId.Value && s.DeletedAt == null);

        if (req.CategoryId.HasValue && req.CategoryId.Value != Guid.Empty)
        {
            spuQuery = spuQuery.Where(s => s.CategoryId == req.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            var keyword = req.Keyword.Trim().ToLower();
            spuQuery = spuQuery.Where(s => s.Name.ToLower().Contains(keyword) || (s.Brand != null && s.Brand.ToLower().Contains(keyword)));
        }

        var total = await spuQuery.LongCountAsync(ct);

        var spus = await spuQuery
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        var spuIds = spus.Select(s => s.Id).ToList();
        var categoryIds = spus.Select(s => s.CategoryId).Distinct().ToList();

        var categories = await dbContext.Categories
            .AsNoTracking()
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var skus = await dbContext.Skus
            .AsNoTracking()
            .Where(s => spuIds.Contains(s.SpuId) && s.DeletedAt == null)
            .ToListAsync(ct);

        var shop = await dbContext.Shops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == shopId.Value, ct);

        var shopName = shop?.ShopName ?? "Gian hàng";

        var items = spus.Select(spu =>
        {
            var spuSkus = skus.Where(s => s.SpuId == spu.Id);
            var categoryName = categories.TryGetValue(spu.CategoryId, out var cName) ? cName : "Danh mục";
            return spu.ToSummaryResponse(shopName, categoryName, spuSkus);
        }).ToList();

        return PagedResult<SpuResponse>.Create(items, page, size, total);
    }
}
