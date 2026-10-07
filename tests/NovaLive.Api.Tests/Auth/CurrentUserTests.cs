using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NovaLive.Api.Auth;

namespace NovaLive.Api.Tests.Auth;

public class CurrentUserTests
{
    [Fact]
    public void IsAuthenticated_WhenUserNotAuthenticated_ShouldReturnFalse()
    {
        // Arrange
        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        contextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var currentUser = new CurrentUser(contextAccessorMock.Object);

        // Act & Assert
        currentUser.IsAuthenticated.Should().BeFalse();
        currentUser.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_WhenUserHasSubClaim_ShouldReturnGuid()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("sub", expectedUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        contextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var currentUser = new CurrentUser(contextAccessorMock.Object);

        // Act & Assert
        currentUser.IsAuthenticated.Should().BeTrue();
        currentUser.UserId.Should().Be(expectedUserId);
    }
}
