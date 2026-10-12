using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Auth.Commands.ResendOtp;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Auth.Commands.ResendOtp;

public sealed class ResendOtpWorkflowTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Resend_FourthRequestIsLimited_RegardlessOfAccountExistence(bool exists)
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        if (exists) await host.AddUser(active: false);
        for (var i = 0; i < 3; i++)
            (await host.Send(new ResendOtpCommand(new("buyer@example.com")))).IsSuccess.Should().BeTrue();
        var result = await host.Send(new ResendOtpCommand(new("buyer@example.com")));
        result.Error.Type.Should().Be(ErrorType.TooManyRequests);
        result.Error.RetryAfterSeconds.Should().Be(900);
    }
}
