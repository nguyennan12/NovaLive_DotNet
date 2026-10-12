using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Auth.Commands.ForgotPassword;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordWorkflowTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ForgotPassword_ReturnsSameSuccessEvenWhenThrottled(bool exists)
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        if (exists) await host.AddUser();
        for (var i = 0; i < 4; i++)
            (await host.Send(new ForgotPasswordCommand(new("buyer@example.com")))).IsSuccess.Should().BeTrue();
        host.Mail.Verify(m => m.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>(), OtpType.ForgotPassword, It.IsAny<CancellationToken>()),
            Times.Exactly(exists ? 3 : 0));
    }
}
