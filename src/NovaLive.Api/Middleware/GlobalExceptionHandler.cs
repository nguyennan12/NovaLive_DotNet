using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLive.Contracts.Common;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment,
    IOptions<JsonOptions> jsonOptions) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var requestId = httpContext.TraceIdentifier;
        var traceId = System.Diagnostics.Activity.Current?.Id;

        logger.LogError(
            exception,
            "Unhandled exception occurred. RequestId: {RequestId}, TraceId: {TraceId}",
            requestId,
            traceId);

        var statusCode = exception switch
        {
            ApplicationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var apiError = new ApiError
        {
            Code = exception switch
            {
                ApplicationException => "APPLICATION_ERROR",
                UnauthorizedAccessException => "UNAUTHORIZED",
                KeyNotFoundException => "NOT_FOUND",
                _ => "INTERNAL_SERVER_ERROR"
            },
            Message = environment.IsDevelopment()
                ? exception.Message
                : "Đã có lỗi xảy ra từ máy chủ.",
            Type = exception switch
            {
                ApplicationException => ErrorType.Invalid,
                UnauthorizedAccessException => ErrorType.Unauthorized,
                KeyNotFoundException => ErrorType.NotFound,
                _ => ErrorType.Unexpected
            }
        };

        var response = ApiResponse<object>.Fail(apiError);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(
            response,
            jsonOptions.Value.JsonSerializerOptions,
            cancellationToken);

        return true;
    }
}
