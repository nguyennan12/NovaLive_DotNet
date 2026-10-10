using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Behaviors;
using NovaLive.Domain.Common;

using NovaLive.Application.UseCases.Auth.Commands.VerifyOtp;
using NovaLive.Application.Tests.UseCases.Auth.Common;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.VerifyOtp;

public sealed class VerifyOtpLoggingTests
{
    [Fact]
    public async Task SlowAuthRequests_DoNotLogRequestPayloadOrSecrets()
    {
        var request = new VerifyOtpCommand(new(Guid.NewGuid(), "819274"));
        await AuthLogAssertions.AssertSlowRequestIsSafe(request, request.Data.OtpCode);
    }
}
