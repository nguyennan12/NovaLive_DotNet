using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Behaviors;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Tests.UseCases.Auth.Common;

internal static class AuthLogAssertions
{
    public static async Task AssertSlowRequestIsSafe<T>(T request, string secret) where T : notnull
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

}
