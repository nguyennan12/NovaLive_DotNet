using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using NovaLive.Api.Auth;
using NovaLive.Api.Controllers;
using NovaLive.Api.Extensions;
using NovaLive.Api.Middleware;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Auth.Commands.Login;
using NovaLive.Application.UseCases.Auth.Queries.GetCurrentUser;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.Auth;
using Serilog;
using Serilog.Core;
using Serilog.Events;
namespace NovaLive.Api.Tests.Auth;

public sealed class AuthHttpTests
{
    [Fact]
    public void AuthController_InheritsSharedBase_AndExposesExactlyTenEndpoints()
    {
        typeof(AuthController).Should().BeDerivedFrom<NovaLive.Api.Controllers.Common.ApiControllerBase>();
        var endpoints = typeof(AuthController).GetMethods()
            .SelectMany(method => method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute), true)
                .Cast<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>())
            .SelectMany(attribute => attribute.HttpMethods.Select(method => $"{method} {attribute.Template}"));
        endpoints.Should().BeEquivalentTo(
            "POST register", "POST verify-otp", "POST resend-otp", "POST login", "POST refresh",
            "POST forgot-password", "POST reset-password", "POST change-password", "POST logout", "GET me");
    }

    [Theory]
    [InlineData(ErrorType.AlreadyExists, 409)]
    [InlineData(ErrorType.Locked, 423)]
    [InlineData(ErrorType.TooManyRequests, 429)]
    public async Task BusinessFailures_AreProblemDetailsWithRetryAfter(ErrorType type, int expectedStatus)
    {
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponse>.Failure(new Error(type, "Auth.Test", "Rejected.") { RetryAfterSeconds = 300 }));
        await using var app = await CreateApp(sender.Object, Mock.Of<ICacheService>());
        using var client = app.GetTestClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("buyer@example.com", "test"));
        ((int)response.StatusCode).Should().Be(expectedStatus);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        response.Headers.RetryAfter!.Delta.Should().Be(TimeSpan.FromMinutes(5));
        (await response.Content.ReadAsStringAsync()).Should().Contain("Auth.Test").And.NotContain("Password");
    }
    [Theory]
    [InlineData("me", "GET")]
    [InlineData("logout", "POST")]
    [InlineData("change-password", "POST")]
    public async Task ProtectedEndpoints_RejectAnonymousRequests(string path, string method)
    {
        await using var app = await CreateApp(Mock.Of<ISender>(), Mock.Of<ICacheService>());
        using var client = app.GetTestClient();
        var request = new HttpRequestMessage(new HttpMethod(method), "/api/v1/auth/" + path)
            { Content = JsonContent.Create(new { refreshToken = "test", oldPassword = "old", newPassword = "new" }) };
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }
    [Fact]
    public async Task RealJwt_IsAcceptedThenRejectedAfterItsJtiIsBlacklisted()
    {
        var userId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserInfoResponse>.Success(new(userId, "buyer@example.com", "Buyer", ["Buyer"], null, [])));
        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var clock = new Mock<IDateTimeProvider>();
        clock.SetupGet(c => c.UtcNow).Returns(DateTimeOffset.UtcNow);
        var token = new JwtTokenService(Options.Create(new JwtOptions
            { Secret = new string('s', 32), Issuer = "tests", Audience = "tests" }), clock.Object)
            .Create(userId, "buyer@example.com", ["Buyer"], null);
        await using var app = await CreateApp(sender.Object, cache.Object);
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        (await client.GetAsync("/api/v1/auth/me")).StatusCode.Should().Be(HttpStatusCode.OK);
        cache.Setup(c => c.ExistsAsync("Blacklist:" + token.Jti, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        (await client.GetAsync("/api/v1/auth/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        sender.Verify(s => s.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task AuthExceptions_DoNotExposeSecretsInHttpResponseOrRequestLogs()
    {
        const string secret = "password-otp-refresh-sensitive-exception";
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(secret));
        var logs = new CaptureSink();
        await using var app = await CreateApp(sender.Object, Mock.Of<ICacheService>(), logs);
        using var client = app.GetTestClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("buyer@example.com", secret));
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        (await response.Content.ReadAsStringAsync()).Should().NotContain(secret);
        logs.Events.Should().Contain(e => e.RenderMessage().Contains("Auth request failed"));
        string.Join("\n", logs.Events.Select(e => e.RenderMessage() + e.Exception)).Should().NotContain(secret);
    }
    private sealed class CaptureSink : ILogEventSink
    {
        public System.Collections.Concurrent.ConcurrentBag<LogEvent> Events { get; } = [];
        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
    private static async Task<WebApplication> CreateApp(ISender sender, ICacheService cache, CaptureSink? logs = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog((_, configuration) =>
        {
            if (logs is not null) configuration.WriteTo.Sink(logs);
        });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            { ["Jwt:Secret"] = new string('s', 32), ["Jwt:Issuer"] = "tests", ["Jwt:Audience"] = "tests" });
        builder.Services.AddSingleton(sender);
        builder.Services.AddSingleton(cache);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddNovaLiveAuthentication(builder.Configuration);
        builder.Services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);
        builder.Services.AddRateLimiter(options => options.AddPolicy("auth", _ =>
            System.Threading.RateLimiting.RateLimitPartition.GetNoLimiter("test")));
        var app = builder.Build();
        app.UseRouting();
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseMiddleware<JwtRoleContextMiddleware>();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapControllers();
        await app.StartAsync();
        return app;
    }
}
