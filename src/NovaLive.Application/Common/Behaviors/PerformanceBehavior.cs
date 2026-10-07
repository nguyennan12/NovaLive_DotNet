using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Auth;

namespace NovaLive.Application.Common.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int PerformanceThresholdMilliseconds = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await next(cancellationToken);

        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > PerformanceThresholdMilliseconds)
        {
            var requestName = typeof(TRequest).Name;
            var userId = currentUser.UserId;

            logger.LogWarning(
                "Long Running Request: {RequestName} ({ElapsedMilliseconds} ms) executed by User {UserId} with payload {@Request}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                userId,
                request);
        }

        return response;
    }
}
