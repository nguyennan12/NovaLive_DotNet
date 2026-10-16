using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Livestreams.Commands.CreatePublisherToken;
using NovaLive.Application.UseCases.Livestreams.Commands.EndLiveSession;
using NovaLive.Application.UseCases.Livestreams.Commands.PinProduct;
using NovaLive.Application.UseCases.Livestreams.Commands.StartLiveSession;
using NovaLive.Application.UseCases.Livestreams.Commands.UnpinProduct;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Api.Controllers.Livestreams;

[Authorize]
[Route("api/v1/seller/livestreams")]
public sealed class SellerLivestreamsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost("start")]
    public async Task<IActionResult> StartLive(
        [FromBody] StartLivestreamRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new StartLiveSessionCommand(request), ct), StatusCodes.Status201Created);

    [HttpPost("{id:guid}/end")]
    public async Task<IActionResult> EndLive(
        Guid id,
        CancellationToken ct) =>
        Respond(await Sender.Send(new EndLiveSessionCommand(id), ct));

    [HttpPost("{id:guid}/pin-product")]
    public async Task<IActionResult> PinProduct(
        Guid id,
        [FromBody] PinProductRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new PinProductCommand(id, request), ct));

    [HttpDelete("{id:guid}/pin-product/{skuId:guid}")]
    public async Task<IActionResult> UnpinProduct(
        Guid id,
        Guid skuId,
        CancellationToken ct) =>
        Respond(await Sender.Send(new UnpinProductCommand(id, skuId), ct));

    [HttpPost("{id:guid}/token")]
    public async Task<IActionResult> RenewPublisherToken(
        Guid id,
        CancellationToken ct) =>
        Respond(await Sender.Send(new CreateLivestreamPublisherTokenCommand(id), ct));
}
