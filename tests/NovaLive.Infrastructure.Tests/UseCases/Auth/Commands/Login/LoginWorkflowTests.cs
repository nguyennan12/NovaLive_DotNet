using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Auth.Commands.Login;
using NovaLive.Application.UseCases.Auth.Errors;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Auth.Commands.Login;

public sealed class LoginWorkflowTests
{
    [Fact]
    public async Task Login_UsesAllBackoffSteps_AndDoesNotCheckPasswordWhileLocked()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser();
        foreach (var minutes in new[] { 5, 15, 60, 1440 })
        {
            for (var i = 1; i < 5; i++)
                (await host.Send(new LoginCommand(new("buyer@example.com", "wrong")))).Error.Type.Should().Be(ErrorType.Unauthorized);
            var locked = await host.Send(new LoginCommand(new("buyer@example.com", "wrong")));
            locked.Error.Type.Should().Be(ErrorType.Locked);
            locked.Error.RetryAfterSeconds.Should().Be(minutes * 60);
            host.Passwords.Invocations.Clear();
            (await host.Send(new LoginCommand(new("buyer@example.com", "Password123")))).Error.Type.Should().Be(ErrorType.Locked);
            host.Passwords.Verify(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            host.Clock.UtcNow += TimeSpan.FromMinutes(minutes);
        }
    }
    [Fact]
    public async Task Login_SuccessClearsCounters_AndStoresOnlyRefreshHash()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser();
        await host.Send(new LoginCommand(new("buyer@example.com", "wrong")));
        await host.Cache.SetAsync("login:lockcount:buyer@example.com", 2L, TimeSpan.FromDays(1));
        var result = await host.Send(new LoginCommand(new("buyer@example.com", "Password123"), "127.0.0.1", new string('x', 700)));
        result.IsSuccess.Should().BeTrue();
        (await host.Cache.ExistsAsync("login:failed:buyer@example.com")).Should().BeFalse();
        (await host.Cache.ExistsAsync("login:lockcount:buyer@example.com")).Should().BeFalse();
        await host.WithDb(async db =>
        {
            var token = await db.RefreshTokens.SingleAsync();
            token.TokenHash.Should().NotBe(result.Value!.RefreshToken).And.HaveLength(64);
            token.DeviceInfo.Should().HaveLength(500);
            token.ExpiresAt.Should().Be(host.Clock.UtcNow.AddDays(30));
        });
    }
    [Fact]
    public async Task Login_UnverifiedIsForbidden_UnknownAndWrongUseSameError()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser(active: false);
        (await host.Send(new LoginCommand(new("buyer@example.com", "Password123")))).Error.Should().Be(AuthErrors.NotVerified);
        var unknown = await host.Send(new LoginCommand(new("unknown@example.com", "wrong")));
        var wrong = await host.Send(new LoginCommand(new("buyer@example.com", "wrong")));
        unknown.Error.Should().Be(wrong.Error);
        host.Passwords.Verify(p => p.VerifyDummy("wrong"), Times.Once);
    }
}
