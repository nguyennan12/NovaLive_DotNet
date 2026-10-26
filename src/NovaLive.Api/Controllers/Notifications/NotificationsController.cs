using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Notifications.Commands.MarkAllNotificationsRead;
using NovaLive.Application.UseCases.Notifications.Commands.MarkNotificationRead;
using NovaLive.Application.UseCases.Notifications.Queries.GetNotifications;
using NovaLive.Contracts.V1.Notifications;

namespace NovaLive.Api.Controllers.Notifications;

[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] GetNotificationsRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetNotificationsQuery(request), ct));

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct) =>
        Respond(await Sender.Send(new MarkNotificationReadCommand(id), ct));

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct) =>
        Respond(await Sender.Send(new MarkAllNotificationsReadCommand(), ct));
}
