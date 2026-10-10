using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Auth.Commands.Login;
using NovaLive.Application.UseCases.Auth.Commands.Logout;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Auth.Commands.Logout;

public sealed class LogoutWorkflowTests
{
    [Fact]
    public async Task Logout_BlacklistsForRemainingTtl_AndDoesNotRevokeAnotherUsersToken()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        host.Authenticate(await host.AddUser());
        await host.AddUser("other@example.com", phone: "0987654321");
        var otherLogin = await host.Send(new LoginCommand(new("other@example.com", "Password123")));
        (await host.Send(new LogoutCommand(new(otherLogin.Value!.RefreshToken)))).IsSuccess.Should().BeTrue();
        await host.WithDb(async db => (await db.RefreshTokens.SingleAsync()).RevokedAt.Should().BeNull());
        (await host.Cache.GetTimeToLiveAsync("Blacklist:" + host.CurrentUser.Object.Jti)).Should().Be(TimeSpan.FromMinutes(12));
    }
}
