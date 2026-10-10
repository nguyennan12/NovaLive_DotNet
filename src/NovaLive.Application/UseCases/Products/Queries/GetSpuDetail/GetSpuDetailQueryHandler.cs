using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Products.Queries.GetSpuDetail;

public sealed class GetSpuDetailQueryHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser)
    : IQueryHandler<GetSpuDetailQuery, SpuDetailResponse>
{
    public async Task<Result<SpuDetailResponse>> Handle(GetSpuDetailQuery query, CancellationToken ct)
    {
        var spuQuery = dbContext.Spus
            .AsNoTracking()
            .Where(s => s.Id == query.SpuId && s.DeletedAt == null);

        if (query.IsSellerView)
        {
            var shopId = currentUser.ShopId;
            if (!shopId.HasValue || shopId.Value == Guid.Empty)
            {
                return ProductErrors.UnauthorizedShop;
            }

            spuQuery = spuQuery.Where(s => s.ShopId == shopId.Value);
        }
        else
        {
            spuQuery = spuQuery.Where(s => s.Status == ProductStatus.Active);
        }

        var spu = await spuQuery.FirstOrDefaultAsync(ct);
        if (spu is null)
        {
            return ProductErrors.NotFound;
        }

        var shop = await dbContext.Shops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == spu.ShopId, ct);

        var category = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == spu.CategoryId, ct);

        var skuQuery = dbContext.Skus
            .AsNoTracking()
            .Where(s => s.SpuId == spu.Id && s.DeletedAt == null);

        if (!query.IsSellerView)
        {
            skuQuery = skuQuery.Where(s => s.IsActive);
        }

        var skus = await skuQuery.ToListAsync(ct);
        var skuIds = skus.Select(s => s.Id).ToList();

        var inventories = await dbContext.Inventories
            .AsNoTracking()
            .Where(i => skuIds.Contains(i.SkuId))
            .ToDictionaryAsync(i => i.SkuId, ct);

        var images = await dbContext.SkuImages
            .AsNoTracking()
            .Where(img => skuIds.Contains(img.SkuId))
            .OrderBy(img => img.DisplayOrder)
            .ToListAsync(ct);

        var attributes = await dbContext.ProductAttributes
            .AsNoTracking()
            .Where(a => a.SpuId == spu.Id)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new ProductAttributeDto(a.AttrName, a.AttrValue))
            .ToListAsync(ct);

        var skuResponses = skus.Select(sku =>
        {
            inventories.TryGetValue(sku.Id, out var inv);
            var skuImgs = images.Where(img => img.SkuId == sku.Id).Select(img => img.ImageUrl);
            return sku.ToResponse(inv, skuImgs);
        }).ToList();

        return spu.ToDetailResponse(
            shop?.ShopName ?? "Gian hàng",
            category?.Name ?? "Danh mục",
            skuResponses,
            attributes);
    }
}
