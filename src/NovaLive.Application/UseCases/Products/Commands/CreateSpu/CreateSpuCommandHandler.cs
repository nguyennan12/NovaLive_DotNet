using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Events;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Products;
using NovaLive.Domain.System;
using InventoryEntity = NovaLive.Domain.Inventory.Inventory;

namespace NovaLive.Application.UseCases.Products.Commands.CreateSpu;

public sealed class CreateSpuCommandHandler(
    ISpuRepository spuRepository,
    ISkuRepository skuRepository,
    IInventoryRepository inventoryRepository,
    ICategoryRepository categoryRepository,
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
        var category = await categoryRepository.GetByIdAsync(req.CategoryId, ct);
        if (category is null)
        {
            return ProductErrors.CategoryNotFound;
        }

        // 2. Kiểm tra trùng lặp mã SKU trong cùng Shop
        var duplicateCodes = new List<string>();
        foreach (var skuDto in req.Skus)
        {
            if (await skuRepository.ExistsSkuCodeAsync(shopId.Value, skuDto.SkuCode, null, ct))
            {
                duplicateCodes.Add(skuDto.SkuCode);
            }
        }

        if (duplicateCodes.Count > 0)
        {
            return ProductErrors.DuplicateSkuCodes(duplicateCodes);
        }

        // 3. Lấy tên Shop để mapping Response
        var shop = await dbContext.Shops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == shopId.Value, ct);

        // 4. Tạo SPU
        var spu = new Spu(
            shopId: shopId.Value,
            categoryId: req.CategoryId,
            name: req.Name.Trim(),
            description: req.Description,
            brand: req.Brand?.Trim(),
            thumbnailUrl: req.ThumbnailUrl,
            attributesConfigJson: req.AttributesConfigJson,
            status: ProductStatus.Active);

        await spuRepository.AddAsync(spu, ct);

        // 5. Tạo Product Attributes
        if (req.Attributes is not null && req.Attributes.Count > 0)
        {
            var attrOrder = 0;
            var attributes = req.Attributes.Select(a => new ProductAttribute(
                spuId: spu.Id,
                attrName: a.Name.Trim(),
                attrValue: a.Value.Trim(),
                displayOrder: attrOrder++)).ToList();

            await dbContext.ProductAttributes.AddRangeAsync(attributes, ct);
        }

        // 6. Tạo danh sách SKUs và Tồn kho khởi tạo
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

            await skuRepository.AddAsync(sku, ct);

            var skuImageUrls = new List<string>();
            if (skuDto.Images is not null && skuDto.Images.Count > 0)
            {
                var displayOrder = 0;
                foreach (var url in skuDto.Images)
                {
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        var skuImage = new SkuImage(
                            skuId: sku.Id,
                            imageUrl: url.Trim(),
                            displayOrder: displayOrder++);
                        await dbContext.SkuImages.AddAsync(skuImage, ct);
                        skuImageUrls.Add(url.Trim());
                    }
                }
            }

            // Tự động tạo bản ghi Inventories
            var inventory = new InventoryEntity(
                skuId: sku.Id,
                shopId: shopId.Value,
                initialStock: skuDto.InitialStock,
                minStock: 5);

            await inventoryRepository.AddAsync(inventory, ct);

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
                operationId: Guid.NewGuid(),
                refType: "InitialImport",
                refId: spu.Id,
                note: "Khởi tạo tồn kho ban đầu khi tạo sản phẩm mới",
                createdBy: currentUser.UserId);

            await inventoryRepository.AddHistoryAsync(history, ct);

            skuResponses.Add(sku.ToResponse(inventory, skuImageUrls));
        }

        // 7. Ghi Outbox Message
        var integrationEvent = new ProductUpdatedIntegrationEvent(
            SpuId: spu.Id,
            ShopId: shopId.Value,
            Action: "Created",
            SkuId: null);

        var outboxMessage = new OutboxMessage("ProductUpdatedEvent", JsonSerializer.Serialize(integrationEvent));
        await dbContext.OutboxMessages.AddAsync(outboxMessage, ct);

        return spu.ToDetailResponse(
            shopName: shop?.ShopName ?? "Gian hàng",
            categoryName: category.Name,
            skus: skuResponses,
            attributes: req.Attributes ?? []);
    }
}
