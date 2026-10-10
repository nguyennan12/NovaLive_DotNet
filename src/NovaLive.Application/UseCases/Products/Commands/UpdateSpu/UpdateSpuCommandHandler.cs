using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Events;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Products.Commands.UpdateSpu;

public sealed class UpdateSpuCommandHandler(
    ISpuRepository spuRepository,
    ISkuRepository skuRepository,
    IInventoryRepository inventoryRepository,
    ICategoryRepository categoryRepository,
    IAppDbContext dbContext,
    ICurrentUser currentUser)
    : ICommandHandler<UpdateSpuCommand, SpuDetailResponse>
{
    public async Task<Result<SpuDetailResponse>> Handle(UpdateSpuCommand command, CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return ProductErrors.UnauthorizedShop;
        }

        var spu = await spuRepository.GetByIdAndShopAsync(command.SpuId, shopId.Value, ct);

        if (spu is null)
        {
            return ProductErrors.NotFound;
        }

        var req = command.Request;

        // Kiểm tra Category tồn tại
        var category = await categoryRepository.GetByIdAsync(req.CategoryId, ct);

        if (category is null)
        {
            return ProductErrors.CategoryNotFound;
        }

        // Cập nhật SPU
        spu.Update(
            name: req.Name.Trim(),
            description: req.Description,
            categoryId: req.CategoryId,
            brand: req.Brand,
            thumbnailUrl: req.ThumbnailUrl,
            attributesConfigJson: req.AttributesConfigJson);

        spuRepository.Update(spu);

        // Cập nhật ProductAttributes
        var oldAttrs = await dbContext.ProductAttributes
            .Where(a => a.SpuId == spu.Id)
            .ToListAsync(ct);

        dbContext.ProductAttributes.RemoveRange(oldAttrs);

        var createdAttributes = new List<ProductAttributeDto>();
        if (req.Attributes != null && req.Attributes.Count > 0)
        {
            var attrOrder = 0;
            foreach (var attr in req.Attributes)
            {
                var productAttr = new ProductAttribute(spu.Id, attr.Name, attr.Value, attrOrder++);
                await dbContext.ProductAttributes.AddAsync(productAttr, ct);
                createdAttributes.Add(new ProductAttributeDto(attr.Name, attr.Value));
            }
        }

        // Ghi OutboxMessage với typed Integration Event
        var integrationEvent = new ProductUpdatedIntegrationEvent(
            SpuId: spu.Id,
            ShopId: shopId.Value,
            Action: "Updated");

        var outboxMessage = new OutboxMessage("ProductUpdatedEvent", JsonSerializer.Serialize(integrationEvent));
        await dbContext.OutboxMessages.AddAsync(outboxMessage, ct);

        // Lấy danh sách SKUs kèm Tồn kho và ảnh
        var skus = await skuRepository.GetBySpuIdAsync(spu.Id, ct);
        var skuIds = skus.Select(s => s.Id).ToList();
        var inventories = await inventoryRepository.GetBySkuIdsAsync(skuIds, ct);

        var images = await dbContext.SkuImages
            .Where(img => skuIds.Contains(img.SkuId))
            .OrderBy(img => img.DisplayOrder)
            .ToListAsync(ct);

        var skuResponses = skus.Select(sku =>
        {
            inventories.TryGetValue(sku.Id, out var inv);
            var skuImgs = images.Where(img => img.SkuId == sku.Id).Select(img => img.ImageUrl);
            return sku.ToResponse(inv, skuImgs);
        }).ToList();

        var shop = await dbContext.Shops.AsNoTracking().FirstOrDefaultAsync(s => s.Id == shopId.Value, ct);

        return spu.ToDetailResponse(shop?.ShopName ?? "Gian hàng", category.Name, skuResponses, createdAttributes);
    }
}
