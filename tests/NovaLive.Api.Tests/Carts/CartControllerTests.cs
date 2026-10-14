using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Carts;
using NovaLive.Application.UseCases.Carts;
using NovaLive.Application.UseCases.Carts.Commands.AddToCart;
using NovaLive.Application.UseCases.Carts.Commands.UpdateCartItem;
using NovaLive.Application.UseCases.Carts.Commands.RemoveCartItem;
using NovaLive.Application.UseCases.Carts.Commands.RemoveSelectedCartItems;
using NovaLive.Application.UseCases.Carts.Queries.GetCart;
using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Common;
namespace NovaLive.Api.Tests.Carts;
public sealed class CartControllerTests
{
    [Fact]
    public void ControllerRequiresAuthorizationAndCanonicalRoute()
    {
        var type = typeof(CartController);
        type.GetCustomAttributes(typeof(AuthorizeAttribute), true).Should().ContainSingle();
        ((RouteAttribute)type.GetCustomAttributes(typeof(RouteAttribute), true).Single()).Template.Should().Be("api/v1/cart");
        type.GetMethods().Where(m => m.DeclaringType == type)
            .Should().OnlyContain(m => m.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Length == 0);
    }
    [Fact]
    public async Task AllActions_DispatchTypedRequestsWithCancellation_AndRespond()
    {
        var sender = new Mock<ISender>(MockBehavior.Strict); var sku = Guid.NewGuid(); var item = Guid.NewGuid();
        var ct = new CancellationTokenSource().Token;
        sender.Setup(s => s.Send(It.IsAny<GetCartQuery>(), ct)).ReturnsAsync(Result.Success(new CartResponse(Guid.NewGuid(), 0, 0m, [])));
        sender.Setup(s => s.Send(It.Is<AddToCartCommand>(c => c.Request.SkuId == sku && c.Request.Quantity == 2), ct))
            .ReturnsAsync(Result<CartItemDto>.Failure(CartErrors.SkuNotFound));
        sender.Setup(s => s.Send(It.Is<UpdateCartItemCommand>(c => c.CartItemId == item && c.Request.Quantity == 0), ct)).ReturnsAsync(Result.Success());
        sender.Setup(s => s.Send(It.Is<RemoveCartItemCommand>(c => c.CartItemId == item), ct)).ReturnsAsync(Result.Success());
        sender.Setup(s => s.Send(It.Is<RemoveSelectedCartItemsCommand>(c => c.Request.CartItemIds.Single() == item), ct)).ReturnsAsync(Result.Success(1));
        var controller = new CartController(sender.Object);
        ((ObjectResult)await controller.Get(ct)).StatusCode.Should().Be(200);
        ((ObjectResult)await controller.Add(new(sku, 2), ct)).StatusCode.Should().Be(404);
        ((ObjectResult)await controller.Update(item, new(0), ct)).StatusCode.Should().Be(200);
        ((ObjectResult)await controller.Remove(item, ct)).StatusCode.Should().Be(200);
        ((ObjectResult)await controller.RemoveSelected(new([item]), ct)).StatusCode.Should().Be(200);
        sender.VerifyAll();
    }
}
