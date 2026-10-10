using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Behaviors;
using NovaLive.Domain.Common;

using NovaLive.Application.UseCases.Auth.Commands.Refresh;
using NovaLive.Application.Tests.UseCases.Auth.Common;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.Refresh;

public sealed class RefreshLoggingTests
{
    [Fact]
    public async Task SlowAuthRequests_DoNotLogRequestPayloadOrSecrets()
    {
        var request = new RefreshTokenCommand(new("refresh-token-secret"));
        await AuthLogAssertions.AssertSlowRequestIsSafe(request, request.Data.RefreshToken);
    }
}
