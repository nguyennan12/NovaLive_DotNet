using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Carts;
using NovaLive.Application.UseCases.Carts.Commands.AddToCart;
using NovaLive.Application.UseCases.Carts.Commands.UpdateCartItem;
using NovaLive.Application.UseCases.Carts.Commands.RemoveCartItem;
using NovaLive.Application.UseCases.Carts.Commands.RemoveSelectedCartItems;
using NovaLive.Application.UseCases.Carts.Queries.GetCart;
using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Carts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;
using NovaLive.Domain.Shops;
using StockEntity = NovaLive.Domain.Inventory.Inventory;
namespace NovaLive.Application.Tests.UseCases.Carts;
public sealed class CartHandlerTests
{
    [Fact]
    public async Task Add_CreatesCartAndSnapshotsCurrentPrice()
    {
        var f = new CartTestContext();
        var result = await f.AddHandler().Handle(new(new(f.Sku.Id, 2)), default);
        result.IsSuccess.Should().BeTrue(); result.Value!.Price.Should().Be(100m);
        f.Cart!.UserId.Should().Be(f.UserId); f.Cart.UpdatedAt.Should().Be(f.Now);
        f.Items.Should().ContainSingle().Which.UnitPrice.Should().Be(100m);
        f.Items[0].ShopId.Should().Be(f.Sku.ShopId); f.Items[0].AddedAt.Should().Be(f.Now);
        f.Carts.Verify(c => c.TrySaveAsync(f.UserId, It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task Add_AccumulatesAndKeepsOriginalSnapshot()
    {
        var f = new CartTestContext(); var item = f.OwnItem();
        var result = await f.AddHandler().Handle(new(new(f.Sku.Id, 3)), default);
        result.Value!.Quantity.Should().Be(5); result.Value.Price.Should().Be(100m);
        item.UnitPrice.Should().Be(50m); f.Items.Should().ContainSingle();
        f.Cart!.UpdatedAt.Should().Be(f.Now);
    }
    [Theory]
    [InlineData("missing")] [InlineData("inactive")] [InlineData("deleted")]
    [InlineData("spuInactive")] [InlineData("spuDeleted")] [InlineData("spuMissing")]
    [InlineData("shopInactive")] [InlineData("shopMissing")] [InlineData("stockMissing")]
    [InlineData("outOfStock")] [InlineData("tooMany")]
    public async Task Add_RejectsUnavailableProductsAndStock(string problem)
    {
        var f = new CartTestContext(); Error error;
        switch (problem)
        {
            case "missing": f.Skus.Setup(s => s.GetByIdAsync(f.Sku.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Sku?)null); error = CartErrors.SkuNotFound; break;
            case "inactive": f.Sku.SetActive(false); error = CartErrors.SkuUnavailable; break;
            case "deleted": f.Sku.SoftDelete(); error = CartErrors.SkuUnavailable; break;
            case "spuInactive": f.Spu.SetStatus(ProductStatus.Inactive); error = CartErrors.SpuUnavailable; break;
            case "spuDeleted": f.Spu.SoftDelete(); error = CartErrors.SpuUnavailable; break;
            case "spuMissing": f.Spus.Setup(s => s.GetByIdAsync(f.Spu.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Spu?)null); error = CartErrors.SpuUnavailable; break;
            case "shopInactive": f.Shop.Suspend(); error = CartErrors.ShopUnavailable; break;
            case "shopMissing": f.Shops.Setup(s => s.GetActiveByIdAsync(f.Shop.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Shop?)null); error = CartErrors.ShopUnavailable; break;
            case "stockMissing": f.Inventories.Setup(s => s.GetBySkuIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, StockEntity>()); error = CartErrors.OutOfStock; break;
            case "outOfStock": f.Stock.Reserve(7); error = CartErrors.OutOfStock; break;
            default: error = CartErrors.InsufficientStock; break;
        }
        (await f.AddHandler().Handle(new(new(f.Sku.Id, problem == "tooMany" ? 8 : 1)), default)).Error.Should().Be(error);
        f.Carts.Verify(c => c.AddItemAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Theory]
    [InlineData(6)] [InlineData(int.MaxValue)]
    public async Task Add_ChecksAccumulatedQuantityWithoutIntegerOverflow(int quantity)
    {
        var f = new CartTestContext(); var item = f.OwnItem();
        (await f.AddHandler().Handle(new(new(f.Sku.Id, quantity)), default)).Error.Should().Be(CartErrors.InsufficientStock);
        item.Quantity.Should().Be(2);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Add_UniqueRace_ReloadsWinnerAndRechecksStock(bool exceedStock)
    {
        var f = new CartTestContext(); f.OwnItem(1);
        var calls = 0;
        f.Carts.Setup(c => c.TrySaveAsync(f.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(() =>
        {
            if (++calls != 1) return true;
            f.Items.Clear(); f.Cart = new Cart(f.UserId, f.Now);
            f.OwnItem(exceedStock ? 7 : 3);
            return false;
        });
        var result = await f.AddHandler().Handle(new(new(f.Sku.Id, 2)), default);
        if (exceedStock) result.Error.Should().Be(CartErrors.InsufficientStock);
        else { result.IsSuccess.Should().BeTrue(); result.Value!.Quantity.Should().Be(5); calls.Should().Be(2); }
        f.Items.Should().ContainSingle();
    }
    [Fact]
    public async Task Add_RepeatedRacesReturnConflict()
    {
        var f = new CartTestContext();
        f.Carts.Setup(c => c.TrySaveAsync(f.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        (await f.AddHandler().Handle(new(new(f.Sku.Id, 1)), default)).Error.Should().Be(CartErrors.ConcurrentChange);
    }
    [Theory]
    [InlineData(0)] [InlineData(7)] [InlineData(8)]
    public async Task Update_DeletesAtZero_UpdatesWithinAvailableStock_RejectsExcess(int quantity)
    {
        var f = new CartTestContext(); var item = f.OwnItem();
        var result = await f.UpdateHandler().Handle(new(item.Id, new(quantity)), default);
        if (quantity > 7) { result.Error.Should().Be(CartErrors.InsufficientStock); item.Quantity.Should().Be(2); }
        else
        {
            result.IsSuccess.Should().BeTrue();
            if (quantity == 0) f.Items.Should().BeEmpty(); else item.Quantity.Should().Be(quantity);
            f.Cart!.UpdatedAt.Should().Be(f.Now);
        }
    }
    [Fact]
    public async Task UpdateZeroAndRemove_AllowRemovalOfUnavailableSku()
    {
        var f = new CartTestContext(); var item = f.OwnItem(); f.Sku.SoftDelete();
        (await f.UpdateHandler().Handle(new(item.Id, new(0)), default)).IsSuccess.Should().BeTrue();
        var next = f.OwnItem();
        (await f.RemoveHandler().Handle(new(next.Id), default)).IsSuccess.Should().BeTrue();
        f.Items.Should().BeEmpty();
    }
    [Fact]
    public async Task ForeignItems_AreNotFoundOrIgnored_AndNeverDeleted()
    {
        var f = new CartTestContext(); f.OwnItem();
        var foreign = new CartItem(Guid.NewGuid(), Guid.NewGuid(), f.Sku.Id, f.Shop.Id, 1, 50m, f.Now);
        f.Items.Add(foreign);
        (await f.UpdateHandler().Handle(new(foreign.Id, new(0)), default)).Error.Should().Be(CartErrors.ItemNotFound);
        (await f.UpdateHandler().Handle(new(foreign.Id, new(1)), default)).Error.Should().Be(CartErrors.ItemNotFound);
        (await f.RemoveHandler().Handle(new(foreign.Id), default)).Error.Should().Be(CartErrors.ItemNotFound);
        (await f.RemoveSelectedHandler().Handle(new(new([foreign.Id, Guid.NewGuid()])), default)).Value.Should().Be(0);
        f.Items.Should().Contain(foreign);
    }
    [Fact]
    public async Task RemoveSelected_CountsOwnedDistinctIds_AndPreservesOthers()
    {
        var f = new CartTestContext(); var item = f.OwnItem();
        var other = new CartItem(Guid.NewGuid(), f.Cart!.Id, Guid.NewGuid(), f.Shop.Id, 1, 20m, f.Now); f.Items.Add(other);
        (await f.RemoveSelectedHandler().Handle(new(new([item.Id, item.Id, Guid.NewGuid()])), default)).Value.Should().Be(1);
        f.Items.Should().Equal(other); f.Cart.UpdatedAt.Should().Be(f.Now);
    }
    [Fact]
    public async Task MissingCart_ReturnsEmptyAndNotFoundWithoutCreating()
    {
        var f = new CartTestContext();
        var cart = (await f.GetHandler().Handle(new(), default)).Value!;
        cart.UserId.Should().Be(f.UserId); cart.ShopGroups.Should().BeEmpty(); cart.GrandTotal.Should().Be(0);
        (await f.UpdateHandler().Handle(new(Guid.NewGuid(), new(1)), default)).Error.Should().Be(CartErrors.ItemNotFound);
        (await f.RemoveHandler().Handle(new(Guid.NewGuid()), default)).Error.Should().Be(CartErrors.ItemNotFound);
        (await f.RemoveSelectedHandler().Handle(new(new([Guid.NewGuid()])), default)).Value.Should().Be(0);
        f.Carts.Verify(c => c.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task Get_GroupsShops_UsesCurrentPricesAndReservedStock_AndBatchesEveryModule()
    {
        var f = new CartTestContext(); f.OwnItem(2, 1m);
        var shop = new Shop(Guid.NewGuid(), "Second shop", "second"); shop.Approve();
        var spu = new Spu(shop.Id, Guid.NewGuid(), "Second", null, null, null, null);
        var sku = new Sku(spu.Id, shop.Id, "SECOND", null, 50m, 30m);
        f.Items.Add(new CartItem(Guid.NewGuid(), f.Cart!.Id, sku.Id, shop.Id, 3, 1m, f.Now));
        f.Skus.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([f.Sku, sku]);
        f.Spus.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([f.Spu, spu]);
        f.Shops.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([f.Shop, shop]);
        var result = (await f.GetHandler().Handle(new(), default)).Value!;
        result.ShopGroups.Should().HaveCount(2); result.TotalItemsCount.Should().Be(2);
        result.GrandTotal.Should().Be(290m);
        var first = result.ShopGroups.Single(g => g.ShopId == f.Shop.Id).Items.Single();
        first.Price.Should().Be(100m); first.AvailableStock.Should().Be(7); first.LineTotal.Should().Be(200m);
        f.Skus.Verify(s => s.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
        f.Spus.Verify(s => s.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
        f.Shops.Verify(s => s.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
        f.Inventories.Verify(s => s.GetBySkuIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task Get_OwnershipScopeAndDeletedReferences_DoNotLeakForeignCart()
    {
        var f = new CartTestContext(); var own = f.OwnItem();
        f.Items.Add(new CartItem(Guid.NewGuid(), Guid.NewGuid(), f.Sku.Id, f.Shop.Id, 99, 1m, f.Now));
        (await f.GetHandler().Handle(new(), default)).Value!.TotalItemsCount.Should().Be(1);
        f.Sku.SoftDelete();
        (await f.GetHandler().Handle(new(), default)).Value!.ShopGroups.Should().BeEmpty();
        f.Items.Should().Contain(own);
    }
    [Fact]
    public async Task EmptyExistingCart_AvoidsCatalogQueries()
    {
        var f = new CartTestContext(); f.Cart = new Cart(f.UserId, f.Now);
        (await f.GetHandler().Handle(new(), default)).Value!.TotalItemsCount.Should().Be(0);
        f.Skus.Verify(s => s.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task EveryHandler_RejectsUnauthenticatedIdentity()
    {
        var f = new CartTestContext(); f.User.SetupGet(u => u.IsAuthenticated).Returns(false);
        (await f.GetHandler().Handle(new(), default)).Error.Should().Be(CartErrors.Unauthorized);
        (await f.AddHandler().Handle(new(new(f.Sku.Id, 1)), default)).Error.Should().Be(CartErrors.Unauthorized);
        (await f.UpdateHandler().Handle(new(Guid.NewGuid(), new(1)), default)).Error.Should().Be(CartErrors.Unauthorized);
        (await f.RemoveHandler().Handle(new(Guid.NewGuid()), default)).Error.Should().Be(CartErrors.Unauthorized);
        (await f.RemoveSelectedHandler().Handle(new(new([Guid.NewGuid()])), default)).Error.Should().Be(CartErrors.Unauthorized);
    }
}
