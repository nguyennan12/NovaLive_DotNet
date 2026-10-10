using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Inventory.Commands.AdjustInventory;
using NovaLive.Application.UseCases.Inventory.Queries.GetInventoryHistories;
using NovaLive.Application.UseCases.Inventory.Queries.GetSellerInventory;
using NovaLive.Contracts.V1.Products;

namespace NovaLive.Api.Controllers;

[Authorize]
[Route("api/v1/seller/inventory")]
public sealed class SellerInventoryController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetSellerInventory(
        [FromQuery] GetSellerInventoryRequest data,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetSellerInventoryQuery(data), ct));

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustInventory(
        [FromBody] AdjustInventoryRequest data,
        CancellationToken ct) =>
        Respond(await Sender.Send(new AdjustInventoryCommand(data), ct));

    [HttpGet("histories")]
    public async Task<IActionResult> GetInventoryHistories(
        [FromQuery] GetInventoryHistoriesRequest data,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetInventoryHistoriesQuery(data), ct));
}
