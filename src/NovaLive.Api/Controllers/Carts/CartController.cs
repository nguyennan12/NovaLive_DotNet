using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Carts.Commands.AddToCart;
using NovaLive.Application.UseCases.Carts.Commands.UpdateCartItem;
using NovaLive.Application.UseCases.Carts.Commands.RemoveCartItem;
using NovaLive.Application.UseCases.Carts.Commands.RemoveSelectedCartItems;
using NovaLive.Application.UseCases.Carts.Queries.GetCart;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Carts;
namespace NovaLive.Api.Controllers.Carts;
[Authorize]
[Route("api/v1/cart")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
public sealed class CartController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CartResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Respond(await Sender.Send(new GetCartQuery(), ct));

    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<CartItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new AddToCartCommand(request), ct));

    [HttpPut("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid cartItemId, [FromBody] UpdateCartItemRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateCartItemCommand(cartItemId, request), ct));

    [HttpDelete("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Remove(Guid cartItemId, CancellationToken ct) =>
        Respond(await Sender.Send(new RemoveCartItemCommand(cartItemId), ct));

    [HttpDelete("items")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveSelected([FromBody] RemoveCartItemsRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new RemoveSelectedCartItemsCommand(request), ct));
}
