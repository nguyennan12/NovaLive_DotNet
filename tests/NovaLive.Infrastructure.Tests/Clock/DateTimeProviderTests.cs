using FluentAssertions;
using NovaLive.Infrastructure.Services;

namespace NovaLive.Infrastructure.Tests.Clock;

public class DateTimeProviderTests
{
    [Fact]
    public void UtcNow_ShouldReturnCurrentUtcTime()
    {
        // Arrange
        var provider = new DateTimeProvider();

        // Act
        var before = DateTimeOffset.UtcNow;
        var now = provider.UtcNow;
        var after = DateTimeOffset.UtcNow;

        // Assert
        now.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}
