using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Products;

namespace NovaLive.Infrastructure.Persistence.Seeding;

public sealed class ProductDataSeeder(
    AppDbContext dbContext,
    ILogger<ProductDataSeeder> logger) : IDataSeeder
{
    public int Order => 3;

    private static readonly Guid FlagshipShopId = Guid.Parse("a1111111-1111-1111-1111-111111111111");
    private static readonly Guid PhonesCatId = Guid.Parse("11111111-1111-1111-1111-111111111103");
    private static readonly Guid ElectronicsCatId = Guid.Parse("11111111-1111-1111-1111-111111111104");

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting Product data seeding from catalog backup...");

        var shopExists = await dbContext.Shops.AnyAsync(s => s.Id == FlagshipShopId, cancellationToken);
        if (!shopExists)
        {
            logger.LogWarning("Flagship shop does not exist. Skipping product seeding.");
            return;
        }

        var sampleProducts = GetSampleCatalog();

        var existingSpuNames = await dbContext.Spus
            .Select(s => s.Name)
            .ToHashSetAsync(cancellationToken);

        var existingSkuCodes = await dbContext.Skus
            .Select(s => s.SkuCode)
            .ToHashSetAsync(cancellationToken);

        var addedSpuCount = 0;
        var addedSkuCount = 0;

        foreach (var p in sampleProducts)
        {
            if (existingSpuNames.Contains(p.Name))
            {
                continue;
            }

            // Entity tự động sinh Id = Guid.NewGuid() qua base constructor
            var spu = new Spu(
                shopId: FlagshipShopId,
                categoryId: p.CategoryId,
                name: p.Name,
                description: p.Description,
                brand: p.Brand,
                thumbnailUrl: p.ThumbnailUrl,
                attributesConfigJson: p.AttributesConfigJson,
                status: ProductStatus.Active);

            await dbContext.Spus.AddAsync(spu, cancellationToken);
            addedSpuCount++;

            foreach (var skuDto in p.Skus)
            {
                if (existingSkuCodes.Contains(skuDto.SkuCode))
                {
                    continue;
                }

                // Sku cũng tự sinh Id, liên kết spuId từ spu.Id vừa tạo
                var sku = new Sku(
                    spuId: spu.Id,
                    shopId: FlagshipShopId,
                    skuCode: skuDto.SkuCode,
                    attributesJson: skuDto.AttributesJson,
                    originalPrice: skuDto.OriginalPrice,
                    sellPrice: skuDto.SellPrice,
                    weightGram: skuDto.WeightGram,
                    isActive: true);

                await dbContext.Skus.AddAsync(sku, cancellationToken);
                addedSkuCount++;

                // Inventory quản lý tồn kho cho sku.Id
                var inventory = new Inventory(
                    skuId: sku.Id,
                    shopId: FlagshipShopId,
                    initialStock: skuDto.Stock,
                    minStock: 5);

                await dbContext.Inventories.AddAsync(inventory, cancellationToken);
            }
        }

        if (addedSpuCount > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Seeded {SpuCount} sample SPUs and {SkuCount} SKUs with inventories successfully.",
                addedSpuCount,
                addedSkuCount);
        }
        else
        {
            logger.LogInformation("All sample products already exist. Skipping product seeding.");
        }
    }

    private sealed record ProductSeedDto(
        Guid CategoryId,
        string Name,
        string Description,
        string Brand,
        string ThumbnailUrl,
        string? AttributesConfigJson,
        List<SkuSeedDto> Skus);

    private sealed record SkuSeedDto(
        string SkuCode,
        string AttributesJson,
        decimal OriginalPrice,
        decimal SellPrice,
        int WeightGram,
        int Stock);

    private static List<ProductSeedDto> GetSampleCatalog()
    {
        return
        [
            new(
                CategoryId: PhonesCatId,
                Name: "Điện thoại Apple iPhone 15 Pro Max",
                Description: "Apple iPhone 15 Pro Max 256GB chính hãng VN/A, khung Titan chuẩn hàng không vũ trụ, chip A17 Pro mạnh mẽ vượt trội, camera tele 5x.",
                Brand: "Apple",
                ThumbnailUrl: "https://res.cloudinary.com/nguyennan12/image/upload/c_fill,h_600,w_600/v1/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/hrijqhs9g0glczumx7kn.jpg?_a=BAMAOGfk0",
                AttributesConfigJson: JsonSerializer.Serialize(new { Color = new[] { "Titan Đen", "Titan Trắng" }, Storage = new[] { "256GB" } }),
                Skus:
                [
                    new(
                        SkuCode: "IPHONE-15PM-DEN",
                        AttributesJson: JsonSerializer.Serialize(new { Color = "Titan Đen", Storage = "256GB" }),
                        OriginalPrice: 24990000m,
                        SellPrice: 22990000m,
                        WeightGram: 800,
                        Stock: 35),
                    new(
                        SkuCode: "IPHONE-15PM-TRANG",
                        AttributesJson: JsonSerializer.Serialize(new { Color = "Titan Trắng", Storage = "256GB" }),
                        OriginalPrice: 24990000m,
                        SellPrice: 22990000m,
                        WeightGram: 800,
                        Stock: 50)
                ]
            ),
            new(
                CategoryId: PhonesCatId,
                Name: "Điện thoại Samsung Galaxy S24 Ultra",
                Description: "Samsung Galaxy S24 Ultra 5G AI thông minh thế hệ mới, màn hình Dynamic AMOLED 2X 120Hz, bút S-Pen quyền năng, camera 200MP zoom quang 100x.",
                Brand: "Samsung",
                ThumbnailUrl: "https://res.cloudinary.com/nguyennan12/image/upload/c_fill,h_600,w_600/v1/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/vnjdxza89j9lxm2fxo9s.jpg?_a=BAMAOGfk0",
                AttributesConfigJson: JsonSerializer.Serialize(new { Color = new[] { "Xám Titan", "Đen Titan" }, Storage = new[] { "256GB" } }),
                Skus:
                [
                    new(
                        SkuCode: "S24U-XAM-256",
                        AttributesJson: JsonSerializer.Serialize(new { Color = "Xám Titan", Storage = "256GB" }),
                        OriginalPrice: 25990000m,
                        SellPrice: 21990000m,
                        WeightGram: 800,
                        Stock: 40),
                    new(
                        SkuCode: "S24U-DEN-256",
                        AttributesJson: JsonSerializer.Serialize(new { Color = "Đen Titan", Storage = "256GB" }),
                        OriginalPrice: 25990000m,
                        SellPrice: 21990000m,
                        WeightGram: 800,
                        Stock: 45)
                ]
            ),
            new(
                CategoryId: ElectronicsCatId,
                Name: "Laptop Apple MacBook Pro 14 M3",
                Description: "Laptop Apple MacBook Pro 14 inch M3 (8-core CPU, 10-core GPU, 8GB Unified Memory, 512GB SSD), màn hình Liquid Retina XDR 120Hz rực rỡ, pin lên tới 22 giờ.",
                Brand: "Apple",
                ThumbnailUrl: "https://res.cloudinary.com/nguyennan12/image/upload/c_fill,h_600,w_600/v1/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/pdmlr4dssgu4mwygvtir.jpg?_a=BAMAOGfk0",
                AttributesConfigJson: JsonSerializer.Serialize(new { Color = new[] { "Space Gray" }, RAM = new[] { "8GB" } }),
                Skus:
                [
                    new(
                        SkuCode: "MBP14-M3-512",
                        AttributesJson: JsonSerializer.Serialize(new { Color = "Space Gray", RAM = "8GB" }),
                        OriginalPrice: 34990000m,
                        SellPrice: 30990000m,
                        WeightGram: 1600,
                        Stock: 25)
                ]
            ),
            new(
                CategoryId: ElectronicsCatId,
                Name: "Laptop Gaming ASUS ROG Strix G16",
                Description: "Laptop Gaming ASUS ROG Strix G16 G614JVR (Intel Core i9-14900HX, RTX 4080 12GB, 32GB DDR5, 1TB PCIe 4.0 SSD), màn hình 16 inch 2.5K 240Hz 100% DCI-P3.",
                Brand: "ASUS",
                ThumbnailUrl: "https://res.cloudinary.com/nguyennan12/image/upload/c_fill,h_600,w_600/v1/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/ovxdpih8lwpugl5zyj65.jpg?_a=BAMAOGfk0",
                AttributesConfigJson: JsonSerializer.Serialize(new { Color = new[] { "Eclipse Gray" } }),
                Skus:
                [
                    new(
                        SkuCode: "ROG-G16-RTX4080",
                        AttributesJson: JsonSerializer.Serialize(new { Color = "Eclipse Gray" }),
                        OriginalPrice: 42990000m,
                        SellPrice: 38990000m,
                        WeightGram: 2500,
                        Stock: 15)
                ]
            ),
            new(
                CategoryId: PhonesCatId,
                Name: "Đồng hồ thông minh Apple Watch Series 9",
                Description: "Apple Watch Series 9 GPS 45mm viền nhôm dây cao su, chip S9 SiP tiên tiến nhất, cử chỉ chạm hai lần Double Tap kỳ diệu, màn hình sáng gấp đôi 2000 nits.",
                Brand: "Apple",
                ThumbnailUrl: "https://res.cloudinary.com/nguyennan12/image/upload/c_fill,h_600,w_600/v1/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/qk7r39n2yeppmaszk0yu.jpg?_a=BAMAOGfk0",
                AttributesConfigJson: JsonSerializer.Serialize(new { Size = new[] { "45mm" }, Color = new[] { "Midnight" } }),
                Skus:
                [
                    new(
                        SkuCode: "AW-S9-45MM-DEN",
                        AttributesJson: JsonSerializer.Serialize(new { Size = "45mm", Color = "Midnight" }),
                        OriginalPrice: 10990000m,
                        SellPrice: 8990000m,
                        WeightGram: 300,
                        Stock: 40)
                ]
            ),
            new(
                CategoryId: PhonesCatId,
                Name: "Tai nghe Apple AirPods Pro 2 USB-C",
                Description: "Tai nghe chống ồn Apple AirPods Pro (2nd Gen) phiên bản hộp sạc MagSafe USB-C, chip H2 độc quyền, chống ồn chủ động ANC đỉnh cao, âm thanh vòm Spatial Audio.",
                Brand: "Apple",
                ThumbnailUrl: "https://res.cloudinary.com/nguyennan12/image/upload/c_fill,h_600,w_600/v1/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/ucwpwj9hg8anb72sz3mn.jpg?_a=BAMAOGfk0",
                AttributesConfigJson: JsonSerializer.Serialize(new { Color = new[] { "Trắng" } }),
                Skus:
                [
                    new(
                        SkuCode: "AIRPODS-PRO-2-USBC",
                        AttributesJson: JsonSerializer.Serialize(new { Color = "Trắng" }),
                        OriginalPrice: 6190000m,
                        SellPrice: 5190000m,
                        WeightGram: 250,
                        Stock: 60)
                ]
            ),
            new(
                CategoryId: PhonesCatId,
                Name: "Tai nghe không dây Sony WH-1000XM5",
                Description: "Tai nghe chụp tai chống ồn Sony WH-1000XM5 hàng đầu thế giới, 8 micro khử tiếng ồn với bộ xử lý Auto NC Optimizer, âm thanh chuẩn Hi-Res Audio Wireless.",
                Brand: "Sony",
                ThumbnailUrl: "https://res.cloudinary.com/nguyennan12/image/upload/c_fill,h_600,w_600/v1/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/vlpy8bdqoabbvtjtga59.jpg?_a=BAMAOGfk0",
                AttributesConfigJson: JsonSerializer.Serialize(new { Color = new[] { "Đen", "Bạc" } }),
                Skus:
                [
                    new(
                        SkuCode: "SONY-WH1000XM5-DEN",
                        AttributesJson: JsonSerializer.Serialize(new { Color = "Đen" }),
                        OriginalPrice: 8490000m,
                        SellPrice: 6990000m,
                        WeightGram: 450,
                        Stock: 30),
                    new(
                        SkuCode: "SONY-WH1000XM5-BAC",
                        AttributesJson: JsonSerializer.Serialize(new { Color = "Bạc" }),
                        OriginalPrice: 8490000m,
                        SellPrice: 6990000m,
                        WeightGram: 450,
                        Stock: 25)
                ]
            )
        ];
    }
}
