using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Extensions;
using NovaLive.Contracts.Common;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Controllers.Common;

[ApiController]
public abstract class ApiControllerBase(ISender sender) : ControllerBase
{
    protected readonly ISender Sender = sender;

    protected IActionResult Respond<T>(
        Result<T> result,
        int successStatusCode = StatusCodes.Status200OK,
        string? message = null)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.Ok(result.Value, message);
            return StatusCode(successStatusCode, response);
        }

        return result.ToActionResult();
    }

    protected IActionResult Respond(
        Result result,
        int successStatusCode = StatusCodes.Status200OK,
        string? message = null)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<object>.Ok(new object(), message ?? "Thao tác thành công.");
            return StatusCode(successStatusCode, response);
        }

        return result.ToActionResult();
    }
}
