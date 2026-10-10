using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;

namespace NovaLive.Application.UseCases.Products.Queries.GetPublicProductDetail;

public sealed class GetPublicProductDetailQueryHandler(
    ISpuRepository spuRepository,
    ISkuRepository skuRepository,
    IInventoryRepository inventoryRepository,
    ICategoryRepository categoryRepository,
    IAppDbContext dbContext)
    : IQueryHandler<GetPublicProductDetailQuery, PublicSpuDetailResponse>
{
    public async Task<Result<PublicSpuDetailResponse>> Handle(GetPublicProductDetailQuery query, CancellationToken ct)
    {
        var spu = await spuRepository.GetByIdAsync(query.SpuId, ct);
        if (spu is null || spu.Status != ProductStatus.Active)
        {
            return ProductErrors.NotFound;
        }

        var shop = await dbContext.Shops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == spu.ShopId, ct);

        var category = await categoryRepository.GetByIdAsync(spu.CategoryId, ct);

        var allSkus = await skuRepository.GetBySpuIdAsync(spu.Id, ct);
        var activeSkus = allSkus.Where(s => s.IsActive).ToList();
        var skuIds = activeSkus.Select(s => s.Id).ToList();

        var inventories = await inventoryRepository.GetBySkuIdsAsync(skuIds, ct);

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

        var skuResponses = activeSkus.Select(sku =>
        {
            inventories.TryGetValue(sku.Id, out var inv);
            var skuImgs = images.Where(img => img.SkuId == sku.Id).Select(img => img.ImageUrl);
            return sku.ToPublicResponse(inv, skuImgs);
        }).ToList();

        return spu.ToPublicDetailResponse(
            shop?.ShopName ?? "Gian hàng",
            category?.Name ?? "Danh mục",
            skuResponses,
            attributes);
    }
}
