using FluentAssertions;
using NovaLive.Application.UseCases.Products;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Products;
using Xunit;

namespace NovaLive.Application.Tests.Products;

public class ProductDetailMappingTests
{
    [Fact]
    public void ToPublicDetailResponse_ShouldNotExposeQtyOnHandOrReservedQty_AndComputeInStock()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var spu = new Spu(
            shopId: shopId,
            categoryId: categoryId,
            name: "Test Spu",
            description: "Test Desc",
            brand: "Brand",
            thumbnailUrl: "https://example.com/img.jpg",
            attributesConfigJson: "[]");

        var skuInStock = new Sku(
            spuId: spu.Id,
            shopId: shopId,
            skuCode: "SKU-IN-STOCK",
            attributesJson: "{}",
            originalPrice: 100000m,
            sellPrice: 80000m,
            weightGram: 100,
            isActive: true);

        var skuOutOfStock = new Sku(
            spuId: spu.Id,
            shopId: shopId,
            skuCode: "SKU-OUT-OF-STOCK",
            attributesJson: "{}",
            originalPrice: 100000m,
            sellPrice: 80000m,
            weightGram: 100,
            isActive: true);

        var invInStock = new Domain.Inventory.Inventory(skuInStock.Id, shopId, 50, 5);
        invInStock.AdjustReserved(10); // Available = 40

        var invOutOfStock = new Domain.Inventory.Inventory(skuOutOfStock.Id, shopId, 10, 5);
        invOutOfStock.AdjustReserved(10); // Available = 0

        var publicSku1 = skuInStock.ToPublicResponse(invInStock);
        var publicSku2 = skuOutOfStock.ToPublicResponse(invOutOfStock);

        // Act
        var publicResponse = spu.ToPublicDetailResponse(
            shopName: "Shop A",
            categoryName: "Cat A",
            skus: [publicSku1, publicSku2],
            attributes: []);

        // Assert
        publicResponse.Should().BeOfType<PublicSpuDetailResponse>();
        publicResponse.Skus.Should().HaveCount(2);

        var res1 = publicResponse.Skus.Single(s => s.SkuCode == "SKU-IN-STOCK");
        res1.InStock.Should().BeTrue();
        res1.AvailableQty.Should().Be(40);

        var res2 = publicResponse.Skus.Single(s => s.SkuCode == "SKU-OUT-OF-STOCK");
        res2.InStock.Should().BeFalse();
        res2.AvailableQty.Should().Be(0);
    }

    [Fact]
    public void ToDetailResponse_ForSeller_ShouldExposeQtyOnHandAndReservedQty()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var spu = new Spu(
            shopId: shopId,
            categoryId: categoryId,
            name: "Test Spu",
            description: "Test Desc",
            brand: "Brand",
            thumbnailUrl: "https://example.com/img.jpg",
            attributesConfigJson: "[]");

        var sku = new Sku(
            spuId: spu.Id,
            shopId: shopId,
            skuCode: "SKU-SELLER",
            attributesJson: "{}",
            originalPrice: 100000m,
            sellPrice: 80000m,
            weightGram: 100,
            isActive: true);

        var inv = new Domain.Inventory.Inventory(sku.Id, shopId, 50, 5);
        inv.AdjustReserved(10); // Available = 40

        var sellerSku = sku.ToResponse(inv);

        // Act
        var sellerResponse = spu.ToDetailResponse(
            shopName: "Shop A",
            categoryName: "Cat A",
            skus: [sellerSku],
            attributes: []);

        // Assert
        sellerResponse.Should().BeOfType<SpuDetailResponse>();
        var res = sellerResponse.Skus.Single();
        res.QtyOnHand.Should().Be(50);
        res.ReservedQty.Should().Be(10);
        res.AvailableQty.Should().Be(40);
    }
}
