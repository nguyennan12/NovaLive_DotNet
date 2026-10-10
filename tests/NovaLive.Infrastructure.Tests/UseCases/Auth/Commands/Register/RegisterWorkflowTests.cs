using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Auth.Commands.Register;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Auth.Commands.Register;

public sealed class RegisterWorkflowTests
{
    [Fact]
    public async Task Registration_NormalizesEmail_HashesOtp_SendsMail_AndRejectsDuplicateEmailOrPhone()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var result = await host.Send(new RegisterCommand(new("  New@Example.com  ", "0912345678", "Password123", "New User")));
        result.IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            var user = await db.Users.SingleAsync();
            user.Email.Should().Be("new@example.com");
            user.AccountStatus.Should().Be(AccountStatus.Unverified);
            user.PasswordHash.Should().NotBe("Password123");
            var otp = await db.UserOtps.SingleAsync();
            otp.OtpHash.Should().HaveLength(64).And.NotBe(host.Codes[user.Email]);
            otp.ExpiresAt.Should().Be(host.Clock.UtcNow.AddMinutes(5));
        });
        host.Mail.Verify(m => m.SendOtpAsync("new@example.com", It.IsAny<string>(), OtpType.EmailVerify, It.IsAny<CancellationToken>()), Times.Once);
        (await host.Send(new RegisterCommand(new("NEW@example.com", "0987654321", "Password123", "Duplicate")))).Error.Type.Should().Be(ErrorType.AlreadyExists);
        (await host.Send(new RegisterCommand(new("other@example.com", "+84912345678", "Password123", "Duplicate")))).Error.Type.Should().Be(ErrorType.AlreadyExists);
    }
    [Fact]
    public async Task EmailFailure_DoesNotRollBackRegistration()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        host.Mail.Setup(m => m.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OtpType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Email provider unavailable"));
        (await host.Send(new RegisterCommand(new("new@example.com", "0912345678", "Password123", "New")))).IsSuccess.Should().BeTrue();
        await host.WithDb(async db => (await db.Users.CountAsync()).Should().Be(1));
    }
}
