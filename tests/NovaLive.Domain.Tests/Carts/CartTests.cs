using FluentAssertions;
using NovaLive.Domain.Carts;
namespace NovaLive.Domain.Tests.Carts;
public sealed class CartTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 0, 0, 0, TimeSpan.Zero);
    [Fact]
    public void AddAndUpdate_AccumulateWithoutChangingSnapshot_AndUseSuppliedTime()
    {
        var cart = new Cart(Guid.NewGuid(), Now);
        var sku = Guid.NewGuid(); var shop = Guid.NewGuid();
        var item = cart.AddItem([], sku, shop, 2, 10m, Now).Value!;
        cart.AddItem([item], sku, shop, 3, 20m, Now.AddMinutes(1)).Value.Should().BeSameAs(item);
        item.Quantity.Should().Be(5); item.UnitPrice.Should().Be(10m);
        item.AddedAt.Should().Be(Now); item.UpdatedAt.Should().Be(Now.AddMinutes(1));
        cart.UpdateItem(item, 4, Now.AddMinutes(2)).IsSuccess.Should().BeTrue();
        cart.UpdatedAt.Should().Be(Now.AddMinutes(2)); item.Quantity.Should().Be(4);
    }
    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(int.MinValue)]
    public void InvalidQuantity_DoesNotMutate(int quantity)
    {
        var cart = new Cart(Guid.NewGuid(), Now);
        cart.AddItem([], Guid.NewGuid(), Guid.NewGuid(), quantity, 10m, Now).Error.Should().Be(CartDomainErrors.InvalidQuantity);
        var item = cart.AddItem([], Guid.NewGuid(), Guid.NewGuid(), 1, 10m, Now).Value!;
        item.SetQuantity(quantity, Now.AddHours(1)).IsFailure.Should().BeTrue();
        item.IncreaseQuantity(quantity, Now.AddHours(1)).IsFailure.Should().BeTrue();
        item.Quantity.Should().Be(1); item.UpdatedAt.Should().Be(Now);
    }
    [Fact]
    public void OverflowAndNegativePrice_ReturnErrors()
    {
        var cart = new Cart(Guid.NewGuid(), Now);
        cart.AddItem([], Guid.NewGuid(), Guid.NewGuid(), 1, -1m, Now).Error.Should().Be(CartDomainErrors.InvalidPrice);
        var item = cart.AddItem([], Guid.NewGuid(), Guid.NewGuid(), int.MaxValue, 1m, Now).Value!;
        item.IncreaseQuantity(1, Now).Error.Should().Be(CartDomainErrors.InvalidQuantity);
        item.Quantity.Should().Be(int.MaxValue);
    }
    [Fact]
    public void Selection_OnlyReturnsOwnedDistinctMatches_AndPreservesUnselectedItems()
    {
        var cart = new Cart(Guid.NewGuid(), Now);
        var own = cart.AddItem([], Guid.NewGuid(), Guid.NewGuid(), 1, 10m, Now).Value!;
        var unselected = cart.AddItem([], Guid.NewGuid(), Guid.NewGuid(), 1, 10m, Now).Value!;
        var foreign = new CartItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, 10m, Now);
        cart.UpdateItem(foreign, 2, Now).Error.Should().Be(CartDomainErrors.ItemNotFound);
        cart.RemoveSelectedItems([own, unselected, foreign], [own.Id, own.Id, foreign.Id, Guid.NewGuid()], Now.AddHours(1))
            .Should().Equal(own);
        foreign.Quantity.Should().Be(1); unselected.Quantity.Should().Be(1);
        cart.RemoveSelectedItems([unselected], [Guid.NewGuid()], Now.AddHours(2)).Should().BeEmpty();
        cart.UpdatedAt.Should().Be(Now.AddHours(1));
    }
}
