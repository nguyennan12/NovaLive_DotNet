using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;

namespace NovaLive.Application.UseCases.Products.Queries.GetSpuDetail;

public sealed class GetSpuDetailQueryHandler(
    ISpuRepository spuRepository,
    ISkuRepository skuRepository,
    IInventoryRepository inventoryRepository,
    ICategoryRepository categoryRepository,
    IAppDbContext dbContext,
    ICurrentUser currentUser)
    : IQueryHandler<GetSpuDetailQuery, SpuDetailResponse>
{
    public async Task<Result<SpuDetailResponse>> Handle(GetSpuDetailQuery query, CancellationToken ct)
    {
        Spu? spu;

        if (query.IsSellerView)
        {
            var shopId = currentUser.ShopId;
            if (!shopId.HasValue || shopId.Value == Guid.Empty)
            {
                return ProductErrors.UnauthorizedShop;
            }

            spu = await spuRepository.GetByIdAndShopAsync(query.SpuId, shopId.Value, ct);
        }
        else
        {
            spu = await spuRepository.GetByIdAsync(query.SpuId, ct);
            if (spu is not null && spu.Status != ProductStatus.Active)
            {
                spu = null;
            }
        }

        if (spu is null)
        {
            return ProductErrors.NotFound;
        }

        var shop = await dbContext.Shops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == spu.ShopId, ct);

        var category = await categoryRepository.GetByIdAsync(spu.CategoryId, ct);

        var allSkus = await skuRepository.GetBySpuIdAsync(spu.Id, ct);
        var skus = query.IsSellerView ? allSkus : allSkus.Where(s => s.IsActive).ToList();
        var skuIds = skus.Select(s => s.Id).ToList();

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
