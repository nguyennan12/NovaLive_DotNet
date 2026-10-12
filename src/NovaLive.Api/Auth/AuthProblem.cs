using Microsoft.AspNetCore.Mvc;
using NovaLive.Domain.Common;
namespace NovaLive.Api.Auth;
public static class AuthProblem
{
    public static int Status(ErrorType type) => type switch
    {
        ErrorType.Validation or ErrorType.Invalid => 400,
        ErrorType.Unauthorized => 401, ErrorType.Forbidden => 403,
        ErrorType.NotFound => 404, ErrorType.AlreadyExists => 409,
        ErrorType.Locked => 423, ErrorType.TooManyRequests => 429, _ => 500
    };
    public static ProblemDetails Create(HttpContext context, Error error)
    {
        var status = Status(error.Type);
        var problem = new ProblemDetails
        {
            Status = status, Title = error.Code, Detail = error.Message,
            Instance = context.Request.Path, Type = $"https://httpstatuses.com/{status}"
        };
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (error.RetryAfterSeconds is int seconds)
        {
            context.Response.Headers.RetryAfter = seconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
            problem.Extensions["retryAfterSeconds"] = seconds;
        }
        return problem;
    }
    public static async Task WriteAsync(HttpContext context, Error error)
    {
        var problem = Create(context, error);
        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json",
            cancellationToken: context.RequestAborted);
    }
}

