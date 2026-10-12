using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;
using NovaLive.Domain.Shops;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Services;
using Xunit;

namespace NovaLive.Infrastructure.Tests.UseCases.Products;

public sealed class PostgresProductQueryServiceTests : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public PostgresProductQueryServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .UseSnakeCaseNamingConvention()
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    private AppDbContext CreateContext() => new(_options);

    [Fact]
    public async Task SearchAsync_ShouldReturnOnlyActiveSpusAndActiveShops()
    {
        // Arrange
        await using var context = CreateContext();

        var activeShop = new Shop(Guid.NewGuid(), "Active Shop", "active-shop");
        activeShop.Approve(); // Status = Active
        var pendingShop = new Shop(Guid.NewGuid(), "Pending Shop", "pending-shop"); // Status = Pending

        var category = new Category(Guid.NewGuid(), "Electronics", "electronics", null, null, 1);

        await context.Shops.AddRangeAsync(activeShop, pendingShop);
        await context.Categories.AddAsync(category);

        var activeSpu = new Spu(activeShop.Id, category.Id, "Bàn phím cơ Không dây", "Mô tả", "Logitech", null, null);
        var pendingSpu = new Spu(pendingShop.Id, category.Id, "Bàn phím cơ Có dây", "Mô tả", "Razer", null, null);

        await context.Spus.AddRangeAsync(activeSpu, pendingSpu);

        var activeSku = new Sku(activeSpu.Id, activeShop.Id, "SKU-LOGI-1", "{}", 200000m, 180000m, 500, true);
        var pendingSku = new Sku(pendingSpu.Id, pendingShop.Id, "SKU-RAZ-1", "{}", 250000m, 220000m, 500, true);

        await context.Skus.AddRangeAsync(activeSku, pendingSku);
        await context.SaveChangesAsync();

        var service = new PostgresProductQueryService(context);
        var request = new ProductSearchRequest(Keyword: "Bàn phím");

        // Act
        var result = await service.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be(activeSpu.Id);
        result.Items[0].ShopName.Should().Be("Active Shop");
        result.Total.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByPriceRangeAndCategory()
    {
        // Arrange
        await using var context = CreateContext();

        var shop = new Shop(Guid.NewGuid(), "Gear Shop", "gear-shop");
        shop.Approve();

        var catKeyboards = new Category(Guid.NewGuid(), "Keyboards", "keyboards", null, null, 1);
        var catMice = new Category(Guid.NewGuid(), "Mice", "mice", null, null, 2);

        await context.Shops.AddAsync(shop);
        await context.Categories.AddRangeAsync(catKeyboards, catMice);

        var spu1 = new Spu(shop.Id, catKeyboards.Id, "Phím cơ Custom", null, null, null, null);
        var spu2 = new Spu(shop.Id, catKeyboards.Id, "Phím màng giá rẻ", null, null, null, null);
        var spu3 = new Spu(shop.Id, catMice.Id, "Chuột Gaming", null, null, null, null);

        await context.Spus.AddRangeAsync(spu1, spu2, spu3);

        var sku1 = new Sku(spu1.Id, shop.Id, "SKU-1", "{}", 1000000m, 900000m, 600, true);
        var sku2 = new Sku(spu2.Id, shop.Id, "SKU-2", "{}", 150000m, 120000m, 300, true);
        var sku3 = new Sku(spu3.Id, shop.Id, "SKU-3", "{}", 500000m, 450000m, 100, true);

        await context.Skus.AddRangeAsync(sku1, sku2, sku3);
        await context.SaveChangesAsync();

        var service = new PostgresProductQueryService(context);

        // Act: Filter by category Keyboards and MinPrice = 500,000
        var request = new ProductSearchRequest(
            CategoryId: catKeyboards.Id,
            MinPrice: 500000m);

        var result = await service.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be(spu1.Id);
        result.Items[0].MinPrice.Should().Be(900000m);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
