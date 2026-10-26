using System.Text.Json;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Application.Common.Events;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.System;
using NovaLive.Infrastructure.System;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.Search;

public sealed class ProductUpdatedOutboxTests
{
    [Fact]
    public async Task ExistingT16Payload_PublishesTypedEvent_AndRetriesBeforeMarkingProcessed()
    {
        var bus = new Mock<IPublishEndpoint>();
        var fail = true;
        bus.Setup(p => p.Publish(It.IsAny<ProductUpdatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns((ProductUpdatedEvent _, CancellationToken _) => fail
                ? Task.FromException(new IOException("RabbitMQ offline")) : Task.CompletedTask);
        await using var host = new AuthTestHost();
        await host.InitializeAsync(services => services.AddSingleton(bus.Object));
        var payload = new ProductUpdatedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "SkuUpdated", Guid.NewGuid(), host.Clock.UtcNow);
        await host.WithDb(async db =>
        {
            db.OutboxMessages.Add(new OutboxMessage(nameof(ProductUpdatedEvent), JsonSerializer.Serialize(payload), host.Clock.UtcNow));
            await db.SaveChangesAsync();
        });
        using var worker = new OutboxBackgroundWorker(host.Services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<OutboxBackgroundWorker>.Instance, host.Clock);

        await worker.ProcessOutboxMessagesAsync(default);

        await host.WithDb(async db =>
        {
            var message = await db.OutboxMessages.SingleAsync();
            message.ProcessedAt.Should().BeNull();
            message.RetryCount.Should().Be(1);
            message.ErrorMessage.Should().Be("RabbitMQ offline");
        });
        fail = false;
        await worker.ProcessOutboxMessagesAsync(default);
        await worker.ProcessOutboxMessagesAsync(default);

        await host.WithDb(async db =>
        {
            var message = await db.OutboxMessages.SingleAsync();
            message.ProcessedAt.Should().Be(host.Clock.UtcNow);
            message.ErrorMessage.Should().BeNull();
        });
        bus.Verify(p => p.Publish(It.Is<ProductUpdatedEvent>(e => e.SpuId == payload.SpuId && e.ShopId == payload.ShopId
            && e.Action == payload.Action && e.SkuId == payload.SkuId && e.OccurredAt == payload.OccurredAt),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
