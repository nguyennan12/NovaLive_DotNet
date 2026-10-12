using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Behaviors;
using NovaLive.Domain.Common;
using NovaLive.Domain.Users;

namespace NovaLive.Application.Tests.Behaviors;

public sealed class AuthorizationBehaviorTests
{
    [Theory]
    [InlineData(false, false, ErrorType.Unauthorized)]
    [InlineData(true, false, ErrorType.Forbidden)]
    [InlineData(true, true, ErrorType.None)]
    public async Task Permissions_AreCheckedAgainstCacheAndRepository(bool authenticated, bool allowed, ErrorType expected)
    {
        var userId = Guid.NewGuid();
        var user = new Mock<ICurrentUser>();
        user.SetupGet(x => x.IsAuthenticated).Returns(authenticated);
        user.SetupGet(x => x.UserId).Returns(userId);
        user.SetupGet(x => x.Roles).Returns(["Buyer", "Seller"]);

        var cache = new Mock<ICacheService>();
        cache.Setup(x => x.GetAsync<string[]>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(allowed ? ["products:create_own", "inventory:manage_own"] : ["products:create_own"]);

        var userRepo = new Mock<IUserRepository>();
        var permRepo = new Mock<IPermissionRepository>();

        var called = false;
        var behavior = new AuthorizationBehavior<ProtectedRequest, Result>(user.Object, cache.Object, userRepo.Object, permRepo.Object);
        var result = await behavior.Handle(new(), _ => { called = true; return Task.FromResult(Result.Success()); }, default);
        result.Error.Type.Should().Be(expected);
        called.Should().Be(expected == ErrorType.None);
    }

    [Fact]
    public async Task CacheMiss_LoadsFromRepositories_AndStoresInCache()
    {
        var userId = Guid.NewGuid();
        var user = new Mock<ICurrentUser>();
        user.SetupGet(x => x.IsAuthenticated).Returns(true);
        user.SetupGet(x => x.UserId).Returns(userId);
        user.SetupGet(x => x.Roles).Returns(["Buyer"]);

        var cache = new Mock<ICacheService>();
        cache.Setup(x => x.GetAsync<string[]>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string[]?)null);

        var activeUser = new User("test@example.com", "hash", "Test", "0912345678");
        activeUser.Activate();
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(activeUser, userId);

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeUser);

        var permRepo = new Mock<IPermissionRepository>();
        permRepo.Setup(x => x.GetPermissionCodesAsync(userId, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(["products:create_own", "inventory:manage_own"]);

        var called = false;
        var behavior = new AuthorizationBehavior<ProtectedRequest, Result>(user.Object, cache.Object, userRepo.Object, permRepo.Object);
        var result = await behavior.Handle(new(), _ => { called = true; return Task.FromResult(Result.Success()); }, default);

        result.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
        cache.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string[]>(), TimeSpan.FromMinutes(5), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NoAttribute_DoesNotRequireAuthenticationOrFetchPermissions()
    {
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        var userRepo = new Mock<IUserRepository>(MockBehavior.Strict);
        var permRepo = new Mock<IPermissionRepository>(MockBehavior.Strict);

        var result = await new AuthorizationBehavior<PublicRequest, Result>(Mock.Of<ICurrentUser>(), cache.Object, userRepo.Object, permRepo.Object)
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

        var result = await new AuthorizationBehavior<AdminRequest, Result>(
            user.Object, Mock.Of<ICacheService>(), Mock.Of<IUserRepository>(), Mock.Of<IPermissionRepository>())
            .Handle(new(), _ => Task.FromResult(Result.Success()), default);
        result.IsSuccess.Should().Be(expected);
    }

    [Fact]
    public async Task EmptyRoles_ReturnsForbiddenImmediately()
    {
        var user = new Mock<ICurrentUser>();
        user.SetupGet(x => x.IsAuthenticated).Returns(true);
        user.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        user.SetupGet(x => x.Roles).Returns(Array.Empty<string>());

        var behavior = new AuthorizationBehavior<ProtectedRequest, Result>(
            user.Object, Mock.Of<ICacheService>(), Mock.Of<IUserRepository>(), Mock.Of<IPermissionRepository>());
        var result = await behavior.Handle(new(), _ => Task.FromResult(Result.Success()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }

    [RequirePermission("products:create_own", "inventory:manage_own")]
    private sealed record ProtectedRequest;

    [AuthorizeRole("Admin")]
    private sealed record AdminRequest;

    private sealed record PublicRequest;
}
