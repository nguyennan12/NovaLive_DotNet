using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Application.UseCases.Orders.Commands.ExpireUnpaidPayment;

namespace NovaLive.Infrastructure.System;

public sealed class TimeoutOrderRollbackWorker(
    IServiceScopeFactory scopeFactory,
    IDateTimeProvider clock,
    IOptions<OrderTimeoutOptions> options,
    ILogger<TimeoutOrderRollbackWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.PollInterval);
        try
        {
            do
            {
                try { await RunOnceAsync(stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex) { logger.LogError(ex, "Order timeout scan failed; retry on next tick"); }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    public async Task RunOnceAsync(CancellationToken ct)
    {
        List<Guid> paymentIds;
        using (var scope = scopeFactory.CreateScope())
            paymentIds = await scope.ServiceProvider.GetRequiredService<IPaymentRepository>()
                .GetExpiredOnlineIdsAsync(clock.UtcNow, options.Value.PaymentTimeout, options.Value.BatchSize, ct);

        var failed = 0;
        foreach (var paymentId in paymentIds)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                // Separate scope and command transaction for every payment, including retries next tick.
                using var scope = scopeFactory.CreateScope();
                var result = await scope.ServiceProvider.GetRequiredService<ISender>()
                    .Send(new ExpireUnpaidPaymentCommand(paymentId), ct);
                if (result.IsFailure)
                {
                    failed++;
                    logger.LogError("Order timeout rollback failed for Payment {PaymentId}: {ErrorCode}", paymentId, result.Error.Code);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                failed++;
                logger.LogError(ex, "Order timeout rollback threw for Payment {PaymentId}; retry on next tick", paymentId);
            }
        }
        logger.LogInformation("Order timeout tick: {ExpiredPaymentCount} expired pending payment candidates, {FailedPaymentCount} failures",
            paymentIds.Count, failed);
    }
}
