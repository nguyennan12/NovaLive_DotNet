using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.FlashSales.Queries.GetActiveFlashSales;
using NovaLive.Application.UseCases.FlashSales.Queries.GetFlashSaleProducts;

namespace NovaLive.Api.Controllers.FlashSales;

[Route("api/v1/flash-sales")]
public sealed class FlashSalesController(ISender sender) : ApiControllerBase(sender)
{
    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveFlashSales(CancellationToken ct) =>
        Respond(await Sender.Send(new GetActiveFlashSalesQuery(), ct));

    [AllowAnonymous]
    [HttpGet("{campaignId:guid}/products")]
    public async Task<IActionResult> GetFlashSaleProducts(
        Guid campaignId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetFlashSaleProductsQuery(campaignId, page, size), ct));
}
