using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Behaviors;
using NovaLive.Domain.Common;
namespace NovaLive.Application.Tests.Behaviors;

public sealed class AuthorizationBehaviorTests
{
    [Theory]
    [InlineData(false, false, ErrorType.Unauthorized)]
    [InlineData(true, false, ErrorType.Forbidden)]
    [InlineData(true, true, ErrorType.None)]
    public async Task Permissions_AreRequiredFromProvider(bool authenticated, bool allowed, ErrorType expected)
    {
        var user = new Mock<ICurrentUser>();
        user.SetupGet(x => x.IsAuthenticated).Returns(authenticated);
        user.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        user.SetupGet(x => x.Roles).Returns(["Buyer", "Seller"]);
        var provider = new Mock<IPermissionProvider>();
        provider.Setup(x => x.GetPermissionsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(allowed ? ["products:create_own", "inventory:manage_own"] : ["products:create_own"]);
        var called = false;
        var behavior = new AuthorizationBehavior<ProtectedRequest, Result>(user.Object, provider.Object);
        var result = await behavior.Handle(new(), _ => { called = true; return Task.FromResult(Result.Success()); }, default);
        result.Error.Type.Should().Be(expected);
        called.Should().Be(expected == ErrorType.None);
    }
    [Fact]
    public async Task NoAttribute_DoesNotRequireAuthenticationOrFetchPermissions()
    {
        var provider = new Mock<IPermissionProvider>(MockBehavior.Strict);
        var result = await new AuthorizationBehavior<PublicRequest, Result>(Mock.Of<ICurrentUser>(), provider.Object)
            .Handle(new(), _ => Task.FromResult(Result.Success()), default);
        result.IsSuccess.Should().BeTrue();
    }
    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Buyer", false)]
    public async Task RoleAttribute_RequiresMatchingJwtRole(string role, bool expected)
    {
        var user = new Mock<ICurrentUser>();
        user.SetupGet(x => x.IsAuthenticated).Returns(true);
        user.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        user.SetupGet(x => x.Roles).Returns([role]);
        var result = await new AuthorizationBehavior<AdminRequest, Result>(user.Object, Mock.Of<IPermissionProvider>())
            .Handle(new(), _ => Task.FromResult(Result.Success()), default);
        result.IsSuccess.Should().Be(expected);
    }
    [RequirePermission("products:create_own", "inventory:manage_own")]
    private sealed record ProtectedRequest;
    [AuthorizeRole("Admin")]
    private sealed record AdminRequest;
    private sealed record PublicRequest;
}

