using MediatR;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Application.System.Queries;
using NovaLive.Contracts.Common;

namespace NovaLive.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("")]
    [Route("live")]
    public ActionResult<ApiResponse<object>> Live()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            service = "NovaLive.Api",
            status = "Live",
            timestamp = DateTimeOffset.UtcNow
        }));
    }

    [HttpGet("ready")]
    public async Task<ActionResult<ApiResponse<SystemStatusDto>>> Ready(CancellationToken cancellationToken)
    {
        var status = await sender.Send(new GetSystemStatusQuery(), cancellationToken);
        return Ok(ApiResponse<SystemStatusDto>.Ok(status));
    }
}
