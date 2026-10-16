using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Livestreams.Commands.CreateViewerToken;
using NovaLive.Application.UseCases.Livestreams.Queries.GetActiveLivestreams;
using NovaLive.Application.UseCases.Livestreams.Queries.GetLivestreamById;
using NovaLive.Application.UseCases.Livestreams.Queries.GetPinnedProduct;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Api.Controllers.Livestreams;

[AllowAnonymous]
[Route("api/v1/livestreams")]
public sealed class LivestreamsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveLivestreams(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetActiveLivestreamsQuery(page, size), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLivestreamById(
        Guid id,
        CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetLivestreamByIdQuery(id), ct));

    [HttpGet("{id:guid}/pinned-product")]
    public async Task<IActionResult> GetPinnedProduct(
        Guid id,
        CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetPinnedProductQuery(id), ct));

    [HttpPost("{id:guid}/token")]
    public async Task<IActionResult> GetViewerToken(
        Guid id,
        [FromBody] JoinLiveSessionRequest? request = null,
        CancellationToken ct = default) =>
        Respond(await Sender.Send(new CreateLivestreamViewerTokenCommand(id, request), ct));
}
