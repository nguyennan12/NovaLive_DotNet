using FluentAssertions;
using NovaLive.Domain.Livestreams;
using Xunit;

namespace NovaLive.Domain.Tests.Livestreams;

public class LivestreamProductTests
{
    private readonly Guid _sessionId = Guid.NewGuid();
    private readonly Guid _skuId = Guid.NewGuid();

    [Fact]
    public void Pin_SetsIsPinnedTrueAndRecordsTimestamp()
    {
        var product = new LivestreamProduct(_sessionId, _skuId);
        var now = DateTimeOffset.UtcNow;

        product.Pin(flashPrice: 199_000m, quantityLimit: 50, displayOrder: 1, now);

        product.IsPinned.Should().BeTrue();
        product.FlashPrice.Should().Be(199_000m);
        product.QuantityLimit.Should().Be(50);
        product.DisplayOrder.Should().Be(1);
        product.PinnedAt.Should().Be(now);
    }

    [Fact]
    public void Unpin_SetsIsPinnedFalse()
    {
        var product = new LivestreamProduct(_sessionId, _skuId, 150_000m, 10, 0, true, DateTimeOffset.UtcNow);

        product.Unpin();

        product.IsPinned.Should().BeFalse();
    }

    [Fact]
    public void RecordSold_IncrementsSoldInLive()
    {
        var product = new LivestreamProduct(_sessionId, _skuId);

        product.RecordSold(5);
        product.RecordSold(3);

        product.SoldInLive.Should().Be(8);
    }
}
