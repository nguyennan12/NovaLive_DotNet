using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaLive.Domain.Products;

namespace NovaLive.Infrastructure.Persistence.Seeding;

public sealed class CategoryDataSeeder(
    AppDbContext dbContext,
    ILogger<CategoryDataSeeder> logger) : IDataSeeder
{
    public int Order => 2;

    private static readonly (Guid Id, string Name, string Slug, string IconUrl)[] DefaultCategories =
    [
        (Guid.Parse("11111111-1111-1111-1111-111111111101"), "Thời Trang Nam", "thoi-trang-nam", "https://cdn.novalive.vn/categories/men-fashion.png"),
        (Guid.Parse("11111111-1111-1111-1111-111111111102"), "Thời Trang Nữ", "thoi-trang-nu", "https://cdn.novalive.vn/categories/women-fashion.png"),
        (Guid.Parse("11111111-1111-1111-1111-111111111103"), "Điện Thoại & Phụ Kiện", "dien-thoai-phu-kien", "https://cdn.novalive.vn/categories/phones.png"),
        (Guid.Parse("11111111-1111-1111-1111-111111111104"), "Thiết Bị Điện Tử", "thiet-bi-dien-tu", "https://cdn.novalive.vn/categories/electronics.png"),
        (Guid.Parse("11111111-1111-1111-1111-111111111105"), "Nhà Cửa & Đời Sống", "nha-cua-doi-song", "https://cdn.novalive.vn/categories/home-living.png")
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
