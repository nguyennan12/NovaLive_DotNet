using System.Text.Json;
using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.Services;

namespace NovaLive.Infrastructure.Tests.Shipping;

public sealed class RedisShippingIdempotencyTests
{
    [Fact]
    public async Task WebhookAcknowledgement_RoundTripsThroughJsonCache()
    {
        string? json = null;
        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.SetAsync("idempotency:response:shipping-key", true, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Callback((string _, bool value, TimeSpan? ttl, CancellationToken _) =>
            { ttl.Should().Be(TimeSpan.FromHours(24)); json = JsonSerializer.Serialize(value); }).Returns(Task.CompletedTask);
        cache.Setup(c => c.GetAsync<bool?>("idempotency:response:shipping-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => json is null ? null : JsonSerializer.Deserialize<bool?>(json));
        var redis = new RedisIdempotencyService(cache.Object);
        (await redis.GetResponseAsync<Result>("shipping-key")).Should().BeNull();
        await redis.SaveResponseAsync("shipping-key", Result.Success());
        (await redis.GetResponseAsync<Result>("shipping-key"))!.IsSuccess.Should().BeTrue();
        await redis.SaveResponseAsync("shipping-key", Result.Failure(Error.Failure("Failure", "test")));
        cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
