using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Shops.Commands.AddShopAddress;
using NovaLive.Application.UseCases.Shops.Commands.DeleteShopAddress;
using NovaLive.Application.UseCases.Shops.Commands.UpdateShopAddress;
using NovaLive.Application.UseCases.Shops.Commands.UpdateShopInfo;
using NovaLive.Application.UseCases.Shops.Queries.GetSellerShop;
using NovaLive.Application.UseCases.Shops.Queries.GetShopAddresses;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;

namespace NovaLive.Api.Controllers.Shops;

[Authorize(Roles = "Seller")]
[Route("api/v1/seller/shop")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
public sealed class SellerShopController(ISender sender) : ApiControllerBase(sender)
{
    // Each dispatched request also requires shops:manage_own in AuthorizationBehavior.
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<ShopResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct) =>
        Respond(await Sender.Send(new GetSellerShopQuery(), ct));

    [HttpPut("me")]
    [ProducesResponseType(typeof(ApiResponse<ShopResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMine([FromBody] UpdateShopProfileRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateShopInfoCommand(request), ct));

    [HttpGet("addresses")]
    [ProducesResponseType(typeof(ApiResponse<List<ShopAddressResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAddresses(CancellationToken ct) =>
        Respond(await Sender.Send(new GetShopAddressesQuery(), ct));

    [HttpPost("addresses")]
    [ProducesResponseType(typeof(ApiResponse<ShopAddressResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddAddress([FromBody] CreateShopAddressRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new AddShopAddressCommand(request), ct), StatusCodes.Status201Created);

    [HttpPut("addresses/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShopAddressResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] CreateShopAddressRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateShopAddressCommand(id, request), ct));

    [HttpDelete("addresses/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken ct) =>
        Respond(await Sender.Send(new DeleteShopAddressCommand(id), ct));
}
