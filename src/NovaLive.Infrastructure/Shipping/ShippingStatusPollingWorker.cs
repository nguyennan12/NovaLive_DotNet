using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Application.UseCases.Shipping.Commands.Webhook;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shipping;

namespace NovaLive.Infrastructure.Shipping;

public sealed class ShippingStatusPollingWorker(
    IServiceScopeFactory scopeFactory,
    IDateTimeProvider clock,
    ILogger<ShippingStatusPollingWorker> logger) : BackgroundService
{
    public static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private const int MaxAttempts = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunOnceAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Shipping polling scan failed; retry on next run"); }
            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    public async Task RunOnceAsync(CancellationToken ct)
    {
        var cutoff = clock.UtcNow.AddDays(-3);
        List<ShippingOrder> candidates;
        using (var scope = scopeFactory.CreateScope())
            candidates = await scope.ServiceProvider.GetRequiredService<IShippingOrderRepository>()
                .GetStaleInTransitAsync(cutoff, ct);

        foreach (var shipment in candidates)
        {
            ct.ThrowIfCancellationRequested();
            // Each shipment gets its own scope: failed commands cannot leak tracked state to the next one.
            try
            {
                if (shipment.UpdatedAt >= cutoff || shipment.Status is not (ShippingStatus.Picking or ShippingStatus.Delivering)
                    || string.IsNullOrWhiteSpace(shipment.TrackingCode)) continue;
                using var scope = scopeFactory.CreateScope();
                var services = scope.ServiceProvider;
                var sub = await services.GetRequiredService<ISubOrderRepository>().FindByIdAsync(shipment.SubOrderId, ct);
                if (sub?.Status != SubOrderStatus.Shipping) continue;
                var resolved = services.GetRequiredService<IShippingProviderResolver>().Resolve(shipment.Provider.ToString());
                if (resolved.IsFailure)
                {
                    logger.LogWarning("Shipping polling unavailable for {Provider}: {Error}", shipment.Provider, resolved.Error.Code);
                    continue;
                }

                var tracking = await GetTrackingWithRetryAsync(resolved.Value!, shipment.TrackingCode, ct);
                if (tracking.IsFailure) continue;
                var status = ShippingStatusMapper.Map(shipment.Provider, tracking.Value!.Status);
                if (status is null)
                {
                    logger.LogWarning("Unknown polling status {Status} for {Provider}/{TrackingCode}",
                        tracking.Value.Status, shipment.Provider, shipment.TrackingCode);
                    continue;
                }
                var eventTime = tracking.Value.Events
                    .Where(e => ShippingStatusMapper.Map(shipment.Provider, e.Status) == status)
                    .Select(e => (DateTimeOffset?)e.OccurredAt).Max();
                var result = await services.GetRequiredService<ISender>().Send(new HandleShippingWebhookCommand(
                    shipment.Provider, shipment.TrackingCode, status.Value,
                    JsonSerializer.Serialize(tracking.Value), eventTime), ct);
                if (result.IsFailure)
                    logger.LogWarning("Shipping polling update failed for {TrackingCode}: {Error}", shipment.TrackingCode, result.Error.Code);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Shipping polling failed for {Provider}/{TrackingCode}; retry on next run",
                    shipment.Provider, shipment.TrackingCode);
            }
        }
    }

    private async Task<Result<ShippingTrackingResult>> GetTrackingWithRetryAsync(
        IShippingProvider provider, string trackingCode, CancellationToken ct)
    {
        for (var attempt = 1; ; attempt++)
        {
            Result<ShippingTrackingResult> result;
            try { result = await provider.GetTrackingAsync(trackingCode, ct); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Carrier tracking threw for {TrackingCode}", trackingCode);
                result = ShippingErrors.ProviderError("Shipping.TrackingFailed", "Carrier tracking request failed.");
            }
            if (result.IsSuccess) return result;
            logger.LogWarning("Carrier tracking failed for {TrackingCode}, attempt {Attempt}/{MaxAttempts}: {Error}",
                trackingCode, attempt, MaxAttempts, result.Error.Code);
            if (attempt == MaxAttempts) return result;
            await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct);
        }
    }
}
