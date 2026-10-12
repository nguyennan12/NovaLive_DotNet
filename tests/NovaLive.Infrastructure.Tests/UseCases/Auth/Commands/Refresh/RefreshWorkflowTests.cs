using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Auth.Commands.Login;
using NovaLive.Application.UseCases.Auth.Commands.Refresh;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Auth.Commands.Refresh;

public sealed class RefreshWorkflowTests
{
    [Fact]
    public async Task Refresh_Rotates_AndReuseCommitsRevocationIncludingNewToken()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser();
        var login = await host.Send(new LoginCommand(new("buyer@example.com", "Password123")));
        var rotation = await host.Send(new RefreshTokenCommand(new(login.Value!.RefreshToken)));
        rotation.IsSuccess.Should().BeTrue();
        rotation.Value!.RefreshToken.Should().NotBe(login.Value.RefreshToken);
        var reuse = await host.Send(new RefreshTokenCommand(new(login.Value.RefreshToken)));
        reuse.Error.Type.Should().Be(ErrorType.Unauthorized);
        await host.WithDb(async db => (await db.RefreshTokens.ToListAsync()).Should().HaveCount(2).And.OnlyContain(t => t.RevokedAt != null));
        (await host.Send(new RefreshTokenCommand(new(rotation.Value.RefreshToken)))).Error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
