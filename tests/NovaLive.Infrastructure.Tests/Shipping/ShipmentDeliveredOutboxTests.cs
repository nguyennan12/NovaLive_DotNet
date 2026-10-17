using System.Text.Json;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.System;
using NovaLive.Infrastructure.System;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.Shipping;

public sealed class ShipmentDeliveredOutboxTests
{
    [Fact]
    public async Task PublishFailure_RemainsPendingAndRetriesThenMarksProcessed()
    {
        var bus = new Mock<IPublishEndpoint>(); var fail = true;
        bus.Setup(p => p.Publish(It.IsAny<ShipmentDeliveredEvent>(), It.IsAny<CancellationToken>()))
            .Returns((ShipmentDeliveredEvent _, CancellationToken _) => fail
                ? Task.FromException(new IOException("RabbitMQ offline")) : Task.CompletedTask);
        await using var host = new AuthTestHost(); await host.InitializeAsync(s => s.AddSingleton(bus.Object));
        var delivered = new ShipmentDeliveredEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "GHN", "TRACK-1", host.Clock.UtcNow);
        await host.WithDb(async db =>
        {
            db.OutboxMessages.Add(new OutboxMessage(nameof(ShipmentDeliveredEvent), JsonSerializer.Serialize(delivered), host.Clock.UtcNow));
            await db.SaveChangesAsync();
        });
        using var worker = new OutboxBackgroundWorker(host.Services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<OutboxBackgroundWorker>.Instance, host.Clock);
        await worker.ProcessOutboxMessagesAsync(default);
        await host.WithDb(async db =>
        {
            var message = await db.OutboxMessages.SingleAsync(); message.ProcessedAt.Should().BeNull();
            message.RetryCount.Should().Be(1); message.ErrorMessage.Should().Be("RabbitMQ offline");
        });
        fail = false;
        await worker.ProcessOutboxMessagesAsync(default); await worker.ProcessOutboxMessagesAsync(default);
        await host.WithDb(async db =>
        {
            var message = await db.OutboxMessages.SingleAsync(); message.ProcessedAt.Should().Be(host.Clock.UtcNow);
            message.ErrorMessage.Should().BeNull();
        });
        bus.Verify(p => p.Publish(It.Is<ShipmentDeliveredEvent>(e => e == delivered), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
