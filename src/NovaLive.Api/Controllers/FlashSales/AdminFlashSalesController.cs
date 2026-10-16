using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.FlashSales.Commands.ApproveFlashSaleItem;
using NovaLive.Application.UseCases.FlashSales.Commands.CreateFlashSaleCampaign;
using NovaLive.Application.UseCases.FlashSales.Commands.RejectFlashSaleItem;
using NovaLive.Contracts.V1.FlashSales;

namespace NovaLive.Api.Controllers.FlashSales;

[Authorize]
[Route("api/v1/admin/flash-sales")]
public sealed class AdminFlashSalesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost]
    public async Task<IActionResult> CreateCampaign(
        [FromBody] CreateFlashSaleCampaignRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new CreateFlashSaleCampaignCommand(request), ct), StatusCodes.Status201Created);

    [HttpPut("items/{itemId:guid}/approve")]
    public async Task<IActionResult> ApproveItem(
        Guid itemId,
        CancellationToken ct) =>
        Respond(await Sender.Send(new ApproveFlashSaleItemCommand(itemId), ct));

    [HttpPut("items/{itemId:guid}/reject")]
    public async Task<IActionResult> RejectItem(
        Guid itemId,
        [FromBody] RejectFlashSaleItemRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new RejectFlashSaleItemCommand(itemId, request), ct));
}
