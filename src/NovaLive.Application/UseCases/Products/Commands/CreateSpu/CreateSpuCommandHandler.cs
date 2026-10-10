using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Events;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Products;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Products.Commands.CreateSpu;

public sealed class CreateSpuCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser)
    : ICommandHandler<CreateSpuCommand, SpuDetailResponse>
{
    public async Task<Result<SpuDetailResponse>> Handle(CreateSpuCommand command, CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return ProductErrors.UnauthorizedShop;
        }

        var req = command.Request;

        // 1. Kiểm tra Category tồn tại
        var category = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == req.CategoryId, ct);

        if (category is null)
        {
            return ProductErrors.CategoryNotFound;
        }

        // 2. Kiểm tra trùng lặp mã SKU trong cùng Shop
        var requestedSkuCodes = req.Skus
            .Select(s => s.SkuCode.Trim())
            .ToList();

        var existingSkuCodes = await dbContext.Skus
            .Where(s => s.ShopId == shopId.Value && requestedSkuCodes.Contains(s.SkuCode) && s.DeletedAt == null)
            .Select(s => s.SkuCode)
            .ToListAsync(ct);

        if (existingSkuCodes.Count > 0)
        {
            return ProductErrors.DuplicateSkuCodes(existingSkuCodes);
        }

        // 3. Lấy tên Shop để mapping Response
        var shop = await dbContext.Shops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == shopId.Value, ct);

        var shopName = shop?.ShopName ?? "Gian hàng";

        // 4. Tạo SPU
        var spu = new Spu(
            shopId: shopId.Value,
            categoryId: req.CategoryId,
            name: req.Name.Trim(),
            description: req.Description,
            brand: req.Brand,
            thumbnailUrl: req.ThumbnailUrl,
            attributesConfigJson: req.AttributesConfigJson,
            status: ProductStatus.Active);

        await dbContext.Spus.AddAsync(spu, ct);

        // 5. Tạo ProductAttributes nếu có
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

        // 6. Tạo danh sách SKUs & Tự động khởi tạo tồn kho ban đầu
        var skuResponses = new List<SkuResponse>();

        foreach (var skuDto in req.Skus)
        {
            var sku = new Sku(
                spuId: spu.Id,
                shopId: shopId.Value,
                skuCode: skuDto.SkuCode.Trim(),
                attributesJson: skuDto.AttributesJson,
                originalPrice: skuDto.OriginalPrice,
                sellPrice: skuDto.SellPrice,
                weightGram: skuDto.WeightGram,
                isActive: true);

            await dbContext.Skus.AddAsync(sku, ct);

            // Ảnh SKU
            var skuImageUrls = new List<string>();
            if (skuDto.Images != null && skuDto.Images.Count > 0)
            {
                var imgOrder = 0;
                foreach (var imgUrl in skuDto.Images)
                {
                    var skuImg = new SkuImage(sku.Id, imgUrl, isPrimary: imgOrder == 0, displayOrder: imgOrder++);
                    await dbContext.SkuImages.AddAsync(skuImg, ct);
                    skuImageUrls.Add(imgUrl);
                }
            }

            // Tự động tạo bản ghi Inventories
            var inventory = new Inventory(
                skuId: sku.Id,
                shopId: shopId.Value,
                initialStock: skuDto.InitialStock,
                minStock: 5);

            await dbContext.Inventories.AddAsync(inventory, ct);

            // Ghi nhận sổ cái InventoryHistories
            var history = new InventoryHistory(
                inventoryId: inventory.Id,
                skuId: sku.Id,
                changeType: InventoryChangeType.Import,
                qtyBefore: 0,
                qtyChange: skuDto.InitialStock,
                reservedBefore: 0,
                reservedChange: 0,
                qtyAfter: skuDto.InitialStock,
                refType: "InitialImport",
                refId: spu.Id,
                note: "Khởi tạo tồn kho ban đầu khi tạo sản phẩm mới",
                createdBy: currentUser.UserId);

            await dbContext.InventoryHistories.AddAsync(history, ct);

            skuResponses.Add(sku.ToResponse(inventory, skuImageUrls));
        }

        // 7. Ghi OutboxMessage với typed Integration Event
        var integrationEvent = new ProductUpdatedIntegrationEvent(
            SpuId: spu.Id,
            ShopId: shopId.Value,
            Action: "Created");

        var outboxMessage = new OutboxMessage("ProductUpdatedEvent", JsonSerializer.Serialize(integrationEvent));
        await dbContext.OutboxMessages.AddAsync(outboxMessage, ct);

        return spu.ToDetailResponse(shopName, category.Name, skuResponses, createdAttributes);
    }
}
