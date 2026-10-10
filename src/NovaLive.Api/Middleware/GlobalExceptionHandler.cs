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

        if (httpContext.Request.Path.StartsWithSegments("/api/v1/auth"))
        {
            // Do not serialize third-party error messages that could contain credentials.
            logger.LogError("Auth request failed. RequestId {RequestId}; failure type {FailureType}.", requestId, exception.GetType().Name);
            await NovaLive.Api.Auth.AuthProblem.WriteAsync(httpContext,
                Error.Failure("Auth.ServiceUnavailable", "Unable to complete authentication. Please try again later."));
            return true;
        }

        logger.LogError(
            exception,
            "Unhandled exception occurred. RequestId: {RequestId}, TraceId: {TraceId}",
            requestId,
            traceId);

        var isConflict = exception is Microsoft.EntityFrameworkCore.DbUpdateException
            || exception.InnerException is Npgsql.PostgresException { SqlState: "23505" };

        var statusCode = isConflict
            ? StatusCodes.Status409Conflict
            : exception switch
            {
                ApplicationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

        var apiError = new ApiError
        {
            Code = isConflict
                ? "DATA_CONFLICT"
                : exception switch
                {
                    ApplicationException => "APPLICATION_ERROR",
                    UnauthorizedAccessException => "UNAUTHORIZED",
                    KeyNotFoundException => "NOT_FOUND",
                    _ => "INTERNAL_SERVER_ERROR"
                },
            Message = isConflict
                ? "Dữ liệu bị trùng lặp hoặc xảy ra xung đột bản ghi trong hệ thống."
                : environment.IsDevelopment()
                    ? exception.Message
                    : "Đã có lỗi xảy ra từ máy chủ.",
            Type = isConflict
                ? ErrorType.AlreadyExists
                : exception switch
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
