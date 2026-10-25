using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Dashboards.Seller.GetSellerDashboard;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Dashboards;

namespace NovaLive.Api.Controllers.Reports;

[Authorize(Roles = "Seller")]
[Route("api/v1/seller/reports")]
[Route("seller/reports")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
public sealed class SellerReportsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<SellerDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetSellerDashboardQuery(from, to), ct));
}
