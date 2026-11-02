using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaLive.Domain.Products;

namespace NovaLive.Infrastructure.Persistence.Seeding;

public sealed class CategoryDataSeeder(
    AppDbContext dbContext,
    ILogger<CategoryDataSeeder> logger) : IDataSeeder
{
    public int Order => 2;

    private static readonly (Guid Id, string Name, string Slug, string? IconUrl)[] DefaultCategories =
    [
        (Guid.Parse("c7d6be80-a44f-41de-9bed-a48e270cbece"), "Đồng hồ thông minh", "dong-ho-thong-minh", null),
        (Guid.Parse("9b16a728-d8ad-4ce8-abee-d2c6b58f4ede"), "Lưu trữ", "luu-tru", null),
        (Guid.Parse("28c26258-e23f-47c8-99b3-b87fc29c4086"), "Laptop", "laptop", null),
        (Guid.Parse("4f378f85-0658-47eb-9e66-0c82f6f0ddb6"), "Quần nam", "quan-nam", null),
        (Guid.Parse("67be3df6-abe3-42aa-841b-f9ef7342f208"), "Phụ kiện", "phu-kien", null),
        (Guid.Parse("ab0d1826-24f5-4e2b-a526-0b416df220b0"), "Chuột", "chuot", null),
        (Guid.Parse("11111111-1111-1111-1111-111111111102"), "Thời trang nữ", "thoi-trang-nu", "https://cdn.novalive.vn/categories/women-fashion.png"),
        (Guid.Parse("2e20035b-f99e-4d48-b3ec-c66b5f679be8"), "Túi - Ví nam", "tui-vi-nam", null),
        (Guid.Parse("e628c6f0-813a-43a1-8ae1-549b1ecd3db6"), "Quần nữ", "quan-nu", null),
        (Guid.Parse("5cf2770a-e37e-4b56-b41e-ffd3826bd114"), "Bỉm - Tã", "bim-ta", null),
        (Guid.Parse("6b4d1eb7-00d8-4b59-84e3-fd0d0592a3d0"), "Trang điểm", "trang-diem", null),
        (Guid.Parse("1fecd452-5b95-49a8-803f-4f0b6a3b0eec"), "Loa", "loa", null),
        (Guid.Parse("761bc26a-9644-40a3-88a7-fbf18285ab75"), "Áo nữ", "ao-nu", null),
        (Guid.Parse("e415b6d0-b0e9-4daf-ba62-08a7d2b20e95"), "Giày nữ", "giay-nu", null),
        (Guid.Parse("7d7dfa2b-6c3a-4e59-b584-21ec863cdb3a"), "Sữa & Dinh dưỡng", "sua-dinh-duong", null),
        (Guid.Parse("769a6c92-927a-40ab-b590-e8563e9d7bc3"), "Bếp", "bep", null),
        (Guid.Parse("446e1162-3280-4c2f-b8cd-86b7ca451f85"), "Bàn phím", "ban-phim", null),
        (Guid.Parse("60fb794d-af40-45a6-b039-fd49eb4f6f38"), "Máy tính bảng", "may-tinh-bang", null),
        (Guid.Parse("7b8eee76-97b3-4ce7-ac07-cffcd2cc3e78"), "Điện thoại", "dien-thoai", null),
        (Guid.Parse("68166bc3-43ea-47d2-917a-ff6de9e44daa"), "Mẹ & Bé", "me-va-be", null)
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting Categories data seeding.");

        var categoryIds = DefaultCategories.Select(c => c.Id).ToArray();
        var existingCategoryIds = await dbContext.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToHashSetAsync(cancellationToken);

        var order = 1;
        var addedCount = 0;
        foreach (var cat in DefaultCategories)
        {
            if (!existingCategoryIds.Contains(cat.Id))
            {
                var category = new Category(
                    id: cat.Id,
                    name: cat.Name,
                    slug: cat.Slug,
                    parentId: null,
                    iconUrl: cat.IconUrl,
                    displayOrder: order++,
                    isVisible: true);

                await dbContext.Categories.AddAsync(category, cancellationToken);
                addedCount++;
            }
            else
            {
                order++;
            }
        }

        if (addedCount > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Count} initial categories successfully.", addedCount);
        }
        else
        {
            logger.LogInformation("All default categories already exist. Skipping category seeding.");
        }
    }
}
