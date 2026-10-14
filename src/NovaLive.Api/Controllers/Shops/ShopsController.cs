using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;
using NovaLive.Application.UseCases.Products.Queries.SearchProducts;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Application.UseCases.Shops.Commands.RegisterShop;
using NovaLive.Application.UseCases.Shops.Commands.FollowShop;
using NovaLive.Application.UseCases.Shops.Commands.UnfollowShop;
using NovaLive.Application.UseCases.Shops.Queries.GetMyShop;
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
    [ProducesResponseType(typeof(ApiResponse<ShopResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct) =>
        Respond(await Sender.Send(new GetMyShopQuery(), ct));

    [AllowAnonymous]
    [HttpGet("{shopId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShopDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublic(Guid shopId, CancellationToken ct) =>
        Respond(await Sender.Send(new GetShopPublicQuery(shopId), ct));

    [AllowAnonymous]
    [HttpGet("{shopId:guid}/products")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(Guid shopId, [FromQuery] ProductSearchRequest request,
        [FromQuery(Name = "sort")] string? sort, CancellationToken ct) =>
        Respond(await Sender.Send(new SearchProductsQuery(request with
        {
            ShopId = shopId,
            SortBy = sort ?? request.SortBy
        }), ct));

    [HttpPost("{shopId:guid}/follow")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Follow(Guid shopId, CancellationToken ct) =>
        Respond(await Sender.Send(new FollowShopCommand(shopId), ct));

    [HttpDelete("{shopId:guid}/follow")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Unfollow(Guid shopId, CancellationToken ct) =>
        Respond(await Sender.Send(new UnfollowShopCommand(shopId), ct));
}
