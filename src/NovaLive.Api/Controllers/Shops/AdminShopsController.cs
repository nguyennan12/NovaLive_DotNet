using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Shops.Commands.ApproveShop;
using NovaLive.Application.UseCases.Shops.Commands.BanShop;
using NovaLive.Application.UseCases.Shops.Queries.GetAdminShops;
using NovaLive.Application.UseCases.Wallets.Commands.ApprovePayout;
using NovaLive.Application.UseCases.Wallets.Commands.RejectPayout;
using NovaLive.Application.UseCases.Wallets.Queries.GetAdminPayouts;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;

namespace NovaLive.Api.Controllers.Shops;

[Authorize(Roles = "Admin")]
[Route("api/v1/admin")]
[Route("admin")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
public sealed class AdminShopsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet("shops")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ShopResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShops(
        [FromQuery] GetAdminShopsRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetAdminShopsQuery(request), ct));

    [HttpPut("shops/{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<ShopResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveShop(
        Guid id,
        [FromBody] ApproveShopRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new ApproveShopCommand(id, request), ct));

    [HttpPut("shops/{id:guid}/ban")]
    [ProducesResponseType(typeof(ApiResponse<ShopResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BanShop(
        Guid id,
        [FromBody] BanShopRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new BanShopCommand(id, request), ct));

    [HttpGet("payouts")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SellerPayoutResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayouts(
        [FromQuery] GetAdminPayoutsRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetAdminPayoutsQuery(request), ct));

    [HttpPut("payouts/{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<SellerPayoutResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApprovePayout(
        Guid id,
        [FromBody] ApprovePayoutRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new ApprovePayoutCommand(id, request), ct));

    [HttpPut("payouts/{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<SellerPayoutResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectPayout(
        Guid id,
        [FromBody] RejectPayoutRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new RejectPayoutCommand(id, request), ct));
}
