using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Api.Auth;
using NovaLive.Api.Middleware;
using NovaLive.Application.Abstractions.Services;
namespace NovaLive.Api.Tests.Auth;

public sealed class JwtRoleContextMiddlewareTests
{
    [Theory]
    [InlineData("revoked")]
    [InlineData("redis-down")]
    [InlineData("valid")]
    public async Task AuthenticatedRequest_RejectsRevocationAndCacheFailure(string scenario)
    {
        var jti = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.User = new ClaimsPrincipal(new ClaimsIdentity([
            new("sub", Guid.NewGuid().ToString()), new("jti", jti), new("role", "Buyer"),
            new("exp", DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds().ToString())
        ], "Bearer", "sub", "role"));
        var user = new CurrentUser(new HttpContextAccessor { HttpContext = context });
        var cache = new Mock<ICacheService>();
        var check = cache.Setup(c => c.ExistsAsync("Blacklist:" + jti, It.IsAny<CancellationToken>()));
        if (scenario == "redis-down") check.ThrowsAsync(new InvalidOperationException("unavailable"));
        else check.ReturnsAsync(scenario == "revoked");
        var called = false;
        var middleware = new JwtRoleContextMiddleware(_ => { called = true; return Task.CompletedTask; },
            NullLogger<JwtRoleContextMiddleware>.Instance);
        await middleware.InvokeAsync(context, cache.Object, user);
        called.Should().Be(scenario == "valid");
        if (scenario != "valid")
        {
            context.Response.StatusCode.Should().Be(401);
            context.Response.ContentType.Should().StartWith("application/problem+json");
            Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray()).Should().Contain("Auth.");
        }
    }
    [Fact]
    public async Task AnonymousRequest_DoesNotAccessRedis()
    {
        var context = new DefaultHttpContext();
        var called = false;
        await new JwtRoleContextMiddleware(_ => { called = true; return Task.CompletedTask; },
            NullLogger<JwtRoleContextMiddleware>.Instance).InvokeAsync(context,
            Mock.Of<ICacheService>(MockBehavior.Strict), new CurrentUser(new HttpContextAccessor { HttpContext = context }));
        called.Should().BeTrue();
    }
}

