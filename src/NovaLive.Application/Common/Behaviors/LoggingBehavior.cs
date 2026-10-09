using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUser.UserId;

        logger.LogInformation(
            "Starting request {RequestName} for User {UserId}",
            requestName,
            userId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            if (response is Result { IsFailure: true } result)
            {
                logger.LogWarning(
                    "Request {RequestName} failed with error {@Error} in {ElapsedMilliseconds}ms",
                    requestName,
                    result.Error,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogInformation(
                    "Completed request {RequestName} in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            if (typeof(TRequest).Namespace == "NovaLive.Application.Auth")
            {
                logger.LogError("Auth request {RequestName} failed with exception type {FailureType}.", requestName, ex.GetType().Name);
                throw;
            }
            logger.LogError(
                ex,
                "Request {RequestName} threw an unhandled exception after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
