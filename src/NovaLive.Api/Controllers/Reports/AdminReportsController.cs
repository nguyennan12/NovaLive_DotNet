using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Dashboards.Admin.GetAdminDashboard;

namespace NovaLive.Api.Controllers.Reports;

[Authorize]
[Route("api/v1/admin/reports")]
[Route("admin/reports")]
public sealed class AdminReportsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetAdminDashboardQuery(from, to), ct));
}
