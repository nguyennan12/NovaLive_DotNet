using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.FlashSales.Commands.RegisterSkuForFlashSale;
using NovaLive.Application.UseCases.FlashSales.Queries.GetSellerFlashSaleRegistrations;
using NovaLive.Contracts.V1.FlashSales;

namespace NovaLive.Api.Controllers.FlashSales;

[Authorize]
[Route("api/v1/seller/flash-sales")]
public sealed class SellerFlashSalesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost("{campaignId:guid}/register")]
    public async Task<IActionResult> RegisterSku(
        Guid campaignId,
        [FromBody] RegisterFlashSaleSkuRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new RegisterSkuForFlashSaleCommand(campaignId, request), ct), StatusCodes.Status201Created);

    [HttpGet("registrations")]
    public async Task<IActionResult> GetRegistrations(
        [FromQuery] Guid? campaignId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetSellerFlashSaleRegistrationsQuery(campaignId, status, page, size), ct));
}
