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
using NovaLive.Domain.Inventory;
namespace NovaLive.Application.Tests.UseCases.Carts;
public sealed class CartValidatorTests
{
    [Theory]
    [InlineData(-1, false)] [InlineData(0, false)] [InlineData(1, true)] [InlineData(int.MaxValue, true)]
    public void Add_RequiresPositiveQuantity(int quantity, bool valid) =>
        new AddToCartCommandValidator().Validate(new AddToCartCommand(new(Guid.NewGuid(), quantity))).IsValid.Should().Be(valid);
    [Theory]
    [InlineData(-1, false)] [InlineData(0, true)] [InlineData(1, true)]
    public void Update_AllowsZeroButNotNegative(int quantity, bool valid) =>
        new UpdateCartItemCommandValidator().Validate(new UpdateCartItemCommand(Guid.NewGuid(), new(quantity))).IsValid.Should().Be(valid);
    [Fact]
    public void EmptyIdsNullRequestsAndEmptySelections_AreInvalid()
    {
        new AddToCartCommandValidator().Validate(new AddToCartCommand(new(Guid.Empty, 1))).IsValid.Should().BeFalse();
        new AddToCartCommandValidator().Validate(new AddToCartCommand(null!)).IsValid.Should().BeFalse();
        new UpdateCartItemCommandValidator().Validate(new UpdateCartItemCommand(Guid.Empty, new(1))).IsValid.Should().BeFalse();
        new UpdateCartItemCommandValidator().Validate(new UpdateCartItemCommand(Guid.NewGuid(), null!)).IsValid.Should().BeFalse();
        new RemoveCartItemCommandValidator().Validate(new RemoveCartItemCommand(Guid.Empty)).IsValid.Should().BeFalse();
        new RemoveCartItemCommandValidator().Validate(new RemoveCartItemCommand(Guid.NewGuid())).IsValid.Should().BeTrue();
        var validator = new RemoveSelectedCartItemsCommandValidator();
        validator.Validate(new RemoveSelectedCartItemsCommand(new([]))).IsValid.Should().BeFalse();
        validator.Validate(new RemoveSelectedCartItemsCommand(new([Guid.Empty]))).IsValid.Should().BeFalse();
        validator.Validate(new RemoveSelectedCartItemsCommand(new(null!))).IsValid.Should().BeFalse();
        validator.Validate(new RemoveSelectedCartItemsCommand(null!)).IsValid.Should().BeFalse();
        validator.Validate(new RemoveSelectedCartItemsCommand(new([Guid.NewGuid()]))).IsValid.Should().BeTrue();
        new GetCartQueryValidator().Validate(new GetCartQuery()).IsValid.Should().BeTrue();
    }
    [Fact]
    public void AllRequests_RequireOwnCartPermissionAndCommandsAreTransactional()
    {
        object[] requests = [new GetCartQuery(), new AddToCartCommand(new(Guid.NewGuid(), 1)),
            new UpdateCartItemCommand(Guid.NewGuid(), new(0)), new RemoveCartItemCommand(Guid.NewGuid()),
            new RemoveSelectedCartItemsCommand(new([Guid.NewGuid()]))];
        requests.Should().OnlyContain(r => ((IRequirePermission)r).RequiredPermission == "carts:manage_own");
        requests.Skip(1).Should().OnlyContain(r => r is NovaLive.Application.Common.Messaging.ITransactionalCommand);
    }
}
