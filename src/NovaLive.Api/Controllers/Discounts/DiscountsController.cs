using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Discounts.Queries.GetPlatformDiscounts;
using NovaLive.Application.UseCases.Discounts.Queries.GetShopDiscounts;
using NovaLive.Application.UseCases.Discounts.Queries.ValidateVoucher;
using NovaLive.Contracts.V1.Discounts;

namespace NovaLive.Api.Controllers.Discounts;

[Route("api/v1/discounts")]
public sealed class DiscountsController(ISender sender) : ApiControllerBase(sender)
{
    [AllowAnonymous]
    [HttpGet("shop/{shopId:guid}")]
    public async Task<IActionResult> GetShopDiscounts(Guid shopId, CancellationToken ct) =>
        Respond(await Sender.Send(new GetShopDiscountsQuery(shopId), ct));

    [AllowAnonymous]
    [HttpGet("platform")]
    public async Task<IActionResult> GetPlatformDiscounts(CancellationToken ct) =>
        Respond(await Sender.Send(new GetPlatformDiscountsQuery(), ct));

    [Authorize]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateVoucher(
        [FromBody] ValidateVoucherRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new ValidateVoucherQuery(request), ct));
}
