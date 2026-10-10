using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Application.UseCases.Inventory.Commands.AdjustInventory;
using NovaLive.Application.UseCases.Inventory.Queries.GetInventoryHistories;
using NovaLive.Application.UseCases.Inventory.Queries.GetSellerInventory;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Products;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence.Seeding;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.Inventory;

public sealed class InventoryWorkflowTests
{
    private static async Task<(Guid SellerId, Guid ShopId, Guid SpuId, Guid SkuId)> SeedProductAndInventoryAsync(
        AuthTestHost host,
        int initialOnHand = 20,
        int reservedQty = 5,
        int minStock = 10)
    {
        var seller = new User("seller@example.com", "hash:Password123", "Seller Test", "0988888888");
        seller.Activate();

        var shop = new Shop(seller.Id, "Nova Store", "nova-store");
        shop.Approve();

        var category = new Category(Guid.NewGuid(), "Thời trang", "thoi-trang");

        var spu = new Spu(
            shopId: shop.Id,
            categoryId: category.Id,
            name: "Áo Thun Basic Nova",
            description: "Chất liệu cotton mềm mại",
            brand: "Nova",
            thumbnailUrl: "https://minio.novalive.vn/products/ao-thun.jpg",
            attributesConfigJson: "[]");

        var sku = new Sku(
            spuId: spu.Id,
            shopId: shop.Id,
            skuCode: "AT-BASIC-BLACK-L",
            attributesJson: "{\"Màu\":\"Đen\",\"Size\":\"L\"}",
            originalPrice: 200000m,
            sellPrice: 150000m,
            weightGram: 200,
            isActive: true);

        var inventory = new Domain.Inventory.Inventory(
            skuId: sku.Id,
            shopId: shop.Id,
            initialStock: initialOnHand,
            minStock: minStock);

        if (reservedQty > 0)
        {
            inventory.AdjustReserved(reservedQty);
        }

        await host.WithDb(async db =>
        {
            var seeder = new RbacDataSeeder(db, host.Clock, NullLogger<RbacDataSeeder>.Instance);
            await seeder.SeedAsync(CancellationToken.None);

            db.Users.Add(seller);
            db.UserRoles.Add(new UserRole(seller.Id, SystemRoleIds.Seller, host.Clock.UtcNow));
            db.Shops.Add(shop);
            db.Categories.Add(category);
            db.Spus.Add(spu);
            db.Skus.Add(sku);
            db.Inventories.Add(inventory);
            await db.SaveChangesAsync();
        });

        return (seller.Id, shop.Id, spu.Id, sku.Id);
    }

    private static void SetupSellerAuth(AuthTestHost host, Guid sellerId, Guid shopId)
    {
        host.CurrentUser.SetupGet(x => x.UserId).Returns(sellerId);
        host.CurrentUser.SetupGet(x => x.ShopId).Returns(shopId);
        host.CurrentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        host.CurrentUser.SetupGet(x => x.Roles).Returns(["Seller"]);
        host.CurrentUser.Setup(x => x.HasPermission("inventory:manage_own")).Returns(true);
    }

    [Fact]
    public async Task AdjustInventory_Import_IncreasesOnHandAndCreatesLedgerHistory()
    {
        // Arrange
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var (sellerId, shopId, _, skuId) = await SeedProductAndInventoryAsync(host, initialOnHand: 20, reservedQty: 5);
        SetupSellerAuth(host, sellerId, shopId);

        var command = new AdjustInventoryCommand(new AdjustInventoryRequest(
            SkuId: skuId,
            QtyChange: 15,
            ChangeType: "Import",
            Note: "Nhập thêm hàng đợt 2"));

        // Act
        var result = await host.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.QtyOnHand.Should().Be(35);
        result.Value.ReservedQty.Should().Be(5);
        result.Value.AvailableQty.Should().Be(30);

        await host.WithDb(async db =>
        {
            var inv = await db.Inventories.SingleAsync(i => i.SkuId == skuId);
            inv.QtyOnHand.Should().Be(35);
            inv.ReservedQty.Should().Be(5);

            var history = await db.InventoryHistories.SingleAsync(h => h.SkuId == skuId);
            history.ChangeType.Should().Be(InventoryChangeType.Import);
            history.QtyBefore.Should().Be(20);
            history.QtyChange.Should().Be(15);
            history.QtyAfter.Should().Be(35);
            history.ReservedBefore.Should().Be(5);
            history.ReservedChange.Should().Be(0);
            history.Note.Should().Be("Nhập thêm hàng đợt 2");
            history.CreatedBy.Should().Be(sellerId);

            var outbox = await db.OutboxMessages.SingleAsync(o => o.EventType == "ProductUpdatedEvent");
            outbox.Payload.Should().Contain("InventoryAdjusted");
        });
    }

    [Fact]
    public async Task AdjustInventory_ManualAdjust_DecreasesOnHand_WhenStockIsSufficient()
    {
        // Arrange
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var (sellerId, shopId, _, skuId) = await SeedProductAndInventoryAsync(host, initialOnHand: 20, reservedQty: 5);
        SetupSellerAuth(host, sellerId, shopId);

        var command = new AdjustInventoryCommand(new AdjustInventoryRequest(
            SkuId: skuId,
            QtyChange: -8,
            ChangeType: "ManualAdjust",
            Note: "Hàng lỗi rách bao bì"));

        // Act
        var result = await host.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.QtyOnHand.Should().Be(12);
        result.Value.ReservedQty.Should().Be(5);
        result.Value.AvailableQty.Should().Be(7);

        await host.WithDb(async db =>
        {
            var history = await db.InventoryHistories.SingleAsync(h => h.SkuId == skuId);
            history.ChangeType.Should().Be(InventoryChangeType.ManualAdjust);
            history.QtyBefore.Should().Be(20);
            history.QtyChange.Should().Be(-8);
            history.QtyAfter.Should().Be(12);
        });
    }

    [Fact]
    public async Task AdjustInventory_Rejects_WhenOnHandWouldFallBelowReservedQty()
    {
        // Arrange (Initial: OnHand = 20, Reserved = 15 => QtyChange = -6 causes OnHand 14 < Reserved 15)
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var (sellerId, shopId, _, skuId) = await SeedProductAndInventoryAsync(host, initialOnHand: 20, reservedQty: 15);
        SetupSellerAuth(host, sellerId, shopId);

        var command = new AdjustInventoryCommand(new AdjustInventoryRequest(
            SkuId: skuId,
            QtyChange: -6,
            ChangeType: "ManualAdjust",
            Note: "Giảm quá mức"));

        // Act
        var result = await host.Send(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Inventory.InsufficientStock");

        await host.WithDb(async db =>
        {
            var inv = await db.Inventories.SingleAsync(i => i.SkuId == skuId);
            inv.QtyOnHand.Should().Be(20); // Not modified
            (await db.InventoryHistories.CountAsync()).Should().Be(0); // No ledger written
        });
    }

    [Fact]
    public async Task AdjustInventory_Rejects_WhenUserHasNoShopId()
    {
        // Arrange
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var (sellerId, shopId, _, skuId) = await SeedProductAndInventoryAsync(host);

        // Setup without ShopId
        host.CurrentUser.SetupGet(x => x.UserId).Returns(sellerId);
        host.CurrentUser.SetupGet(x => x.ShopId).Returns((Guid?)null);
        host.CurrentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        host.CurrentUser.SetupGet(x => x.Roles).Returns(["Seller"]);
        host.CurrentUser.Setup(x => x.HasPermission("inventory:manage_own")).Returns(true);

        var command = new AdjustInventoryCommand(new AdjustInventoryRequest(
            SkuId: skuId,
            QtyChange: 5,
            ChangeType: "Import",
            Note: null));

        // Act
        var result = await host.Send(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Inventory.UnauthorizedShop");
    }

    [Fact]
    public async Task GetSellerInventory_ComputesAvailableStock_AndFiltersLowStockCorrectly()
    {
        // Arrange
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var (sellerId, shopId, spuId, skuId1) = await SeedProductAndInventoryAsync(host, initialOnHand: 25, reservedQty: 5, minStock: 10);
        SetupSellerAuth(host, sellerId, shopId);

        // Add a second SKU with low stock (OnHand = 8, Reserved = 2 => Available = 6 <= MinStock 10)
        var sku2 = new Sku(
            spuId: spuId,
            shopId: shopId,
            skuCode: "AT-BASIC-WHITE-M",
            attributesJson: "{\"Màu\":\"Trắng\",\"Size\":\"M\"}",
            originalPrice: 200000m,
            sellPrice: 150000m,
            weightGram: 200,
            isActive: true);

        var inv2 = new Domain.Inventory.Inventory(
            skuId: sku2.Id,
            shopId: shopId,
            initialStock: 8,
            minStock: 10);
        inv2.AdjustReserved(2);

        await host.WithDb(async db =>
        {
            db.Skus.Add(sku2);
            db.Inventories.Add(inv2);
            await db.SaveChangesAsync();
        });

        // Act 1: Get all inventory
        var allQuery = new GetSellerInventoryQuery(new GetSellerInventoryRequest(
            LowStock: null,
            Keyword: null,
            Page: 1,
            Size: 10));
        var allResult = await host.Send(allQuery);

        // Assert 1
        allResult.IsSuccess.Should().BeTrue();
        allResult.Value!.Total.Should().Be(2);

        // Act 2: Filter low stock only
        var lowStockQuery = new GetSellerInventoryQuery(new GetSellerInventoryRequest(
            LowStock: true,
            Keyword: null,
            Page: 1,
            Size: 10));
        var lowStockResult = await host.Send(lowStockQuery);

        // Assert 2: Only sku2 should match (AvailableQty = 6 <= MinStock 10)
        lowStockResult.IsSuccess.Should().BeTrue();
        lowStockResult.Value!.Total.Should().Be(1);
        lowStockResult.Value.Items.Single().SkuCode.Should().Be("AT-BASIC-WHITE-M");
        lowStockResult.Value.Items.Single().AvailableQty.Should().Be(6);
    }

    [Fact]
    public async Task GetInventoryHistories_ReturnsPaginatedLedgerEntries()
    {
        // Arrange
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var (sellerId, shopId, _, skuId) = await SeedProductAndInventoryAsync(host, initialOnHand: 50, reservedQty: 0);
        SetupSellerAuth(host, sellerId, shopId);

        // Perform 2 adjustments
        await host.Send(new AdjustInventoryCommand(new AdjustInventoryRequest(skuId, 10, "Import", "Đợt 1")));
        await host.Send(new AdjustInventoryCommand(new AdjustInventoryRequest(skuId, -5, "ManualAdjust", "Đợt 2")));

        // Act: Query histories for skuId
        var query = new GetInventoryHistoriesQuery(new GetInventoryHistoriesRequest(
            SkuId: skuId,
            ChangeType: null,
            From: null,
            To: null,
            Page: 1,
            Size: 10));

        var result = await host.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);

        // Ordered by CreatedAt DESC: item 0 is ManualAdjust (-5), item 1 is Import (+10)
        result.Value.Items[0].ChangeType.Should().Be("ManualAdjust");
        result.Value.Items[0].QtyChange.Should().Be(-5);
        result.Value.Items[0].QtyAfter.Should().Be(55);

        result.Value.Items[1].ChangeType.Should().Be("Import");
        result.Value.Items[1].QtyChange.Should().Be(10);
        result.Value.Items[1].QtyAfter.Should().Be(60);
    }
}
