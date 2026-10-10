using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Auth;
using NovaLive.Application.Common.Behaviors;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Tests.Behaviors;

public sealed class AuthLoggingTests
{
    [Fact]
    public async Task SlowAuthRequests_DoNotLogRequestPayloadOrSecrets()
    {
        var login = new LoginCommand(new("test@example.com", "Password-secret-123"));
        var verify = new VerifyOtpCommand(new(Guid.NewGuid(), "819274"));
        var refresh = new RefreshTokenCommand(new("refresh-token-secret"));
        await AssertSlowRequestIsSafe(login, login.Data.Password);
        await AssertSlowRequestIsSafe(verify, verify.Data.OtpCode);
        await AssertSlowRequestIsSafe(refresh, refresh.Data.RefreshToken);
    }

    [Fact]
    public async Task AuthExceptions_DoNotLogThirdPartyExceptionMessage()
    {
        var logger = new CaptureLogger<LoggingBehavior<LoginCommand, Result>>();
        var behavior = new LoggingBehavior<LoginCommand, Result>(logger, Mock.Of<ICurrentUser>());
        var secret = "third-party-error-with-sensitive-token";
        Func<Task> act = async () => await behavior.Handle(
            new LoginCommand(new("test@example.com", "Password-secret-123")),
            _ => throw new InvalidOperationException(secret), default);
        await act.Should().ThrowAsync<InvalidOperationException>();
        logger.Messages.Should().NotBeEmpty();
        string.Join("\n", logger.Messages).Should().NotContain(secret).And.NotContain("Password-secret-123");
    }

    private static async Task AssertSlowRequestIsSafe<T>(T request, string secret) where T : notnull
    {
        var logger = new CaptureLogger<PerformanceBehavior<T, Result>>();
        var behavior = new PerformanceBehavior<T, Result>(logger, Mock.Of<ICurrentUser>());
        await behavior.Handle(request, async ct =>
        {
            await Task.Delay(600, ct);
            return Result.Success();
        }, default);
        logger.Messages.Should().ContainSingle();
        logger.Messages.Single().Should().NotContain(secret).And.NotContain("payload");
        logger.Values.Should().NotContain(value => ReferenceEquals(value, request));
    }

    private sealed class CaptureLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];
        public List<object?> Values { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception) + exception?.ToString());
            if (state is IEnumerable<KeyValuePair<string, object?>> fields)
                Values.AddRange(fields.Select(field => field.Value));
        }
    }
}
