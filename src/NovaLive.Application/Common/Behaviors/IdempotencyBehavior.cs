using MediatR;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Idempotency;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Common.Behaviors;

public sealed class IdempotencyBehavior<TRequest, TResponse>(
    IIdempotencyService idempotencyService,
    ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IIdempotentCommand idempotentCommand)
        {
            return await next(cancellationToken);
        }

        var key = idempotentCommand.IdempotencyKey;
        if (string.IsNullOrWhiteSpace(key))
        {
            return await next(cancellationToken);
        }

        var cachedResponse = await idempotencyService.GetResponseAsync<TResponse>(key, cancellationToken);
        if (cachedResponse is not null)
        {
            logger.LogInformation("Returning cached idempotent response for key {IdempotencyKey}", key);
            return cachedResponse;
        }

        var response = await next(cancellationToken);

        if (response is Result { IsSuccess: true })
        {
            await idempotencyService.SaveResponseAsync(key, response, TimeSpan.FromHours(24), cancellationToken);
        }

        return response;
    }
}
