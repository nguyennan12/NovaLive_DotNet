using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Returns.Commands.ApproveReturn;
using NovaLive.Application.UseCases.Returns.Commands.RejectReturn;
using NovaLive.Application.UseCases.Returns.Commands.ConfirmReturnReceived;
using NovaLive.Application.UseCases.Returns.Queries.GetSellerReturns;
using NovaLive.Contracts.V1.Returns;
namespace NovaLive.Api.Controllers.Returns;
[Authorize(Roles = "Seller")]
[Route("api/v1/seller/returns")]
public sealed class SellerReturnsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetSellerReturns([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetSellerReturnsQuery(status, page, size), ct));
    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] SellerReturnResponseRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new ApproveReturnCommand(id, request), ct));
    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReturnRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new RejectReturnCommand(id, request), ct));
    [HttpPut("{id:guid}/received")]
    public async Task<IActionResult> Received(Guid id, CancellationToken ct) => Respond(await Sender.Send(new ConfirmReturnReceivedCommand(id), ct));
}
