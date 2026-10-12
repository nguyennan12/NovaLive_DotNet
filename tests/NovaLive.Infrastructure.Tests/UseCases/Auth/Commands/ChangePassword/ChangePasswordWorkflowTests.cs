using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Auth.Commands.ChangePassword;
using NovaLive.Application.UseCases.Auth.Commands.Login;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Auth.Commands.ChangePassword;

public sealed class ChangePasswordWorkflowTests
{
    [Fact]
    public async Task ChangePassword_RevokesAllRefreshTokensAndBlacklistsCurrentJti()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        host.Authenticate(await host.AddUser());
        await host.Send(new LoginCommand(new("buyer@example.com", "Password123")));
        await host.Send(new LoginCommand(new("buyer@example.com", "Password123")));
        (await host.Send(new ChangePasswordCommand(new("wrong", "NewPassword123")))).Error.Type.Should().Be(ErrorType.Invalid);
        (await host.Send(new ChangePasswordCommand(new("Password123", "Password123")))).Error.Type.Should().Be(ErrorType.Invalid);
        (await host.Send(new ChangePasswordCommand(new("Password123", "NewPassword123")))).IsSuccess.Should().BeTrue();
        await host.WithDb(async db => (await db.RefreshTokens.ToListAsync()).Should().OnlyContain(t => t.RevokedAt != null));
        (await host.Cache.GetTimeToLiveAsync("Blacklist:" + host.CurrentUser.Object.Jti)).Should().Be(TimeSpan.FromMinutes(12));
    }
}
