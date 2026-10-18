using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovaLive.Infrastructure.Persistence;
using System.Text.Json;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Infrastructure.System;

public sealed class OutboxBackgroundWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxBackgroundWorker> logger,
    IDateTimeProvider clock)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);
    private const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxBackgroundWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing Outbox messages.");
            }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }

        logger.LogInformation("OutboxBackgroundWorker stopped.");
    }

    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < 5)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        logger.LogInformation("Processing {Count} outbox messages...", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                if (message.EventType == nameof(ShipmentDeliveredEvent))
                {
                    var delivered = JsonSerializer.Deserialize<ShipmentDeliveredEvent>(message.Payload,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web))
                        ?? throw new JsonException("Invalid shipment delivered event.");
                    await publishEndpoint.Publish(delivered, cancellationToken);
                }
                else if (message.EventType == nameof(OrderPlacedEvent))
                {
                    var orderPlaced = JsonSerializer.Deserialize<OrderPlacedEvent>(message.Payload,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web))
                        ?? throw new JsonException("Invalid order placed event.");
                    await publishEndpoint.Publish(orderPlaced, cancellationToken);
                }
                else if (message.EventType == nameof(PaymentSuccessEvent))
                {
                    var paymentSuccess = JsonSerializer.Deserialize<PaymentSuccessEvent>(message.Payload,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web))
                        ?? throw new JsonException("Invalid payment success event.");
                    await publishEndpoint.Publish(paymentSuccess, cancellationToken);
                }
                // Other event dispatch is owned by the existing event infrastructure tasks.
                message.MarkAsProcessed(clock.UtcNow);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                message.MarkAsFailed(ex.Message);
                logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
