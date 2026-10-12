using FluentAssertions;
using NovaLive.Application.UseCases.Auth.Commands.Logout;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Behaviors;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.Logout;

public sealed class LogoutCommandValidatorTests
{
    // Logout no longer accepts a token/body. Validate the authenticated identity instead.
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    public async Task BodylessLogout_RequiresAuthenticatedUser(bool authenticated, bool hasId, bool allowed)
    {
        var user = new Mock<ICurrentUser>();
        user.SetupGet(x => x.IsAuthenticated).Returns(authenticated);
        user.SetupGet(x => x.UserId).Returns(hasId ? Guid.NewGuid() : null);
        var called = false;
        var result = await new AuthorizationBehavior<LogoutCommand, Result>(user.Object, Mock.Of<IPermissionProvider>())
            .Handle(new LogoutCommand(), _ => { called = true; return Task.FromResult(Result.Success()); }, default);
        called.Should().Be(allowed);
        result.IsSuccess.Should().Be(allowed);
        if (!allowed) result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
