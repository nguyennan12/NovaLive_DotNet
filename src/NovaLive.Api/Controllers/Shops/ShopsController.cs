using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Application.UseCases.Shops.Commands.RegisterShop;
using NovaLive.Application.UseCases.Shops.Commands.UpdateShopInfo;
using NovaLive.Application.UseCases.Shops.Commands.AddShopAddress;
using NovaLive.Application.UseCases.Shops.Commands.UpdateShopAddress;
using NovaLive.Application.UseCases.Shops.Commands.DeleteShopAddress;
using NovaLive.Application.UseCases.Shops.Commands.FollowShop;
using NovaLive.Application.UseCases.Shops.Commands.UnfollowShop;
using NovaLive.Application.UseCases.Shops.Queries.GetMyShop;
using NovaLive.Application.UseCases.Shops.Queries.GetShopAddresses;
using NovaLive.Application.UseCases.Shops.Queries.GetShopPublic;

namespace NovaLive.Api.Controllers.Shops;

[Authorize]
[Route("api/v1/shops")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
public sealed class ShopsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<ShopResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterShopRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new RegisterShopCommand(request), ct), StatusCodes.Status201Created);

    [HttpGet("me")]
    [HttpGet("/api/v1/seller/shop/me")]
    [ProducesResponseType(typeof(ApiResponse<ShopResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct) =>
        Respond(await Sender.Send(new GetMyShopQuery(), ct));

    [HttpPut("me")]
    [HttpPut("/api/v1/seller/shop/me")]
    [ProducesResponseType(typeof(ApiResponse<ShopResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMine([FromBody] UpdateShopProfileRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateShopInfoCommand(request), ct));

    [HttpGet("me/addresses")]
    [HttpGet("/api/v1/seller/shop/addresses")]
    [ProducesResponseType(typeof(ApiResponse<List<ShopAddressResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAddresses(CancellationToken ct) =>
        Respond(await Sender.Send(new GetShopAddressesQuery(), ct));

    [HttpPost("me/addresses")]
    [HttpPost("/api/v1/seller/shop/addresses")]
    [ProducesResponseType(typeof(ApiResponse<ShopAddressResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddAddress([FromBody] CreateShopAddressRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new AddShopAddressCommand(request), ct), StatusCodes.Status201Created);

    [HttpPut("me/addresses/{id:guid}")]
    [HttpPut("/api/v1/seller/shop/addresses/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShopAddressResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] CreateShopAddressRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateShopAddressCommand(id, request), ct));

    [HttpDelete("me/addresses/{id:guid}")]
    [HttpDelete("/api/v1/seller/shop/addresses/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken ct) =>
        Respond(await Sender.Send(new DeleteShopAddressCommand(id), ct));

    [AllowAnonymous]
    [HttpGet("{shopId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShopDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublic(Guid shopId, CancellationToken ct) =>
        Respond(await Sender.Send(new GetShopPublicQuery(shopId), ct));

    [HttpPost("{shopId:guid}/follow")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Follow(Guid shopId, CancellationToken ct) =>
        Respond(await Sender.Send(new FollowShopCommand(shopId), ct));

    [HttpDelete("{shopId:guid}/follow")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Unfollow(Guid shopId, CancellationToken ct) =>
        Respond(await Sender.Send(new UnfollowShopCommand(shopId), ct));
}
