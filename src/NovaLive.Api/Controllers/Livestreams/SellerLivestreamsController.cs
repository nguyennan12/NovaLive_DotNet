using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Livestreams.Commands.CreatePublisherToken;
using NovaLive.Application.UseCases.Livestreams.Commands.CancelScheduledLiveSession;
using NovaLive.Application.UseCases.Livestreams.Commands.EndLiveSession;
using NovaLive.Application.UseCases.Livestreams.Commands.PinProduct;
using NovaLive.Application.UseCases.Livestreams.Commands.StartLiveSession;
using NovaLive.Application.UseCases.Livestreams.Commands.ScheduleLiveSession;
using NovaLive.Application.UseCases.Livestreams.Commands.UpdateScheduledLiveSession;
using NovaLive.Application.UseCases.Livestreams.Commands.StartScheduledLiveSession;
using NovaLive.Application.UseCases.Livestreams.Commands.UnpinProduct;
using NovaLive.Application.UseCases.Livestreams.Queries.GetSellerLivestreams;
using NovaLive.Application.UseCases.Livestreams.Queries.GetSellerLivestreamAnalytics;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Api.Controllers.Livestreams;

[Authorize]
[Route("api/v1/seller/livestreams")]
public sealed class SellerLivestreamsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetSellerLives(
        [FromQuery] GetSellerLivestreamsRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetSellerLivestreamsQuery(request), ct));

    [HttpGet("analytics")]
    public async Task<IActionResult> GetSellerLiveAnalytics(
        [FromQuery] GetSellerLivestreamAnalyticsRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetSellerLivestreamAnalyticsQuery(request), ct));

    [HttpPost("start")]
    public async Task<IActionResult> StartLive(
        [FromBody] StartLivestreamRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new StartLiveSessionCommand(request), ct), StatusCodes.Status201Created);

    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleLive(
        [FromBody] ScheduleLivestreamRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new ScheduleLiveSessionCommand(request), ct), StatusCodes.Status201Created);

    [HttpPut("{id:guid}/schedule")]
    public async Task<IActionResult> UpdateScheduledLive(
        Guid id,
        [FromBody] UpdateScheduledLivestreamRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateScheduledLiveSessionCommand(id, request), ct));

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> StartScheduledLive(
        Guid id,
        CancellationToken ct) =>
        Respond(await Sender.Send(new StartScheduledLiveSessionCommand(id), ct));

    [HttpPost("{id:guid}/end")]
    public async Task<IActionResult> EndLive(
        Guid id,
        CancellationToken ct) =>
        Respond(await Sender.Send(new EndLiveSessionCommand(id), ct));

    [HttpDelete("{id:guid}/schedule")]
    public async Task<IActionResult> CancelScheduledLive(
        Guid id,
        CancellationToken ct) =>
        Respond(await Sender.Send(new CancelScheduledLiveSessionCommand(id), ct));

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
