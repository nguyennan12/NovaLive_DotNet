using NovaLive.Application.Abstractions.Clock;

namespace NovaLive.Infrastructure.Clock;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
