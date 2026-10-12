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
        var userId = await host.AddUser();
        host.Authenticate(userId);
        (await host.Send(new LoginCommand(new("buyer@example.com", "Password123")))).IsSuccess.Should().BeTrue();
        (await host.Send(new LoginCommand(new("buyer@example.com", "Password123")))).IsSuccess.Should().BeTrue();
        await host.AddUser("other@example.com", phone: "0987654321");
        (await host.Send(new LoginCommand(new("other@example.com", "Password123")))).IsSuccess.Should().BeTrue();
        (await host.Send(new LogoutCommand())).IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            var tokens = await db.RefreshTokens.ToListAsync();
            tokens.Where(token => token.UserId == userId).Should().HaveCount(2).And.OnlyContain(token => token.RevokedAt != null);
            tokens.Single(token => token.UserId != userId).RevokedAt.Should().BeNull();
        });
        (await host.Cache.GetTimeToLiveAsync("Blacklist:" + host.CurrentUser.Object.Jti)).Should().Be(TimeSpan.FromMinutes(12));
    }
}
