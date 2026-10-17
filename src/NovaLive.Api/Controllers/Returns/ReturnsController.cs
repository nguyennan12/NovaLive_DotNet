using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Returns.Commands.RequestReturn;
using NovaLive.Application.UseCases.Returns.Queries.GetMyReturns;
using NovaLive.Application.UseCases.Returns.Queries.GetReturnDetail;
using NovaLive.Contracts.V1.Returns;
namespace NovaLive.Api.Controllers.Returns;
[Authorize]
[Route("api/v1/returns")]
public sealed class ReturnsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost]
    public async Task<IActionResult> RequestReturn([FromBody] CreateReturnRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new RequestReturnCommand(request), ct), StatusCodes.Status201Created);
    [HttpGet]
    public async Task<IActionResult> GetMyReturns([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetMyReturnsQuery(status, page, size), ct));
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReturnDetail(Guid id, CancellationToken ct) => Respond(await Sender.Send(new GetReturnDetailQuery(id), ct));
}
