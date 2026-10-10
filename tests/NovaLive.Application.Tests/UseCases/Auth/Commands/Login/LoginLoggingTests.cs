using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Behaviors;
using NovaLive.Domain.Common;

using NovaLive.Application.UseCases.Auth.Commands.Login;
using NovaLive.Application.Tests.UseCases.Auth.Common;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.Login;

public sealed class LoginLoggingTests
{
    [Fact]
    public async Task SlowAuthRequests_DoNotLogRequestPayloadOrSecrets()
    {
        var request = new LoginCommand(new("test@example.com", "Password-secret-123"));
        await AuthLogAssertions.AssertSlowRequestIsSafe(request, request.Data.Password);
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

}
