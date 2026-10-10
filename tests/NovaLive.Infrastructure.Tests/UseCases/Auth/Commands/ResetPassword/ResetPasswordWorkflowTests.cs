using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Auth.Commands.ForgotPassword;
using NovaLive.Application.UseCases.Auth.Commands.Login;
using NovaLive.Application.UseCases.Auth.Commands.ResetPassword;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Auth.Commands.ResetPassword;

public sealed class ResetPasswordWorkflowTests
{
    [Fact]
    public async Task ResetPassword_ConsumesOtp_RevokesTokens_AndClearsLoginLock()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser();
        await host.Send(new LoginCommand(new("buyer@example.com", "Password123")));
        await host.Send(new ForgotPasswordCommand(new("buyer@example.com")));
        await host.Cache.SetAsync("login:lock:buyer@example.com", true, TimeSpan.FromMinutes(5));
        (await host.Send(new ResetPasswordCommand(new("buyer@example.com", host.Codes["buyer@example.com"], "NewPassword123")))).IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            (await db.RefreshTokens.ToListAsync()).Should().OnlyContain(t => t.RevokedAt != null);
            (await db.Users.SingleAsync()).PasswordHash.Should().Be("hash:NewPassword123");
        });
        (await host.Cache.ExistsAsync("login:lock:buyer@example.com")).Should().BeFalse();
    }
}
