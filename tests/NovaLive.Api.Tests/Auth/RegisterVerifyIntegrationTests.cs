using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Infrastructure.Persistence;
using StackExchange.Redis;
using Moq;

namespace NovaLive.Api.Tests.Auth;

[CollectionDefinition("API host lifecycle", DisableParallelization = true)]
public sealed class ApiHostLifecycleCollection { }

[Collection("API host lifecycle")]
public sealed class RegisterVerifyIntegrationTests
{
    private static readonly RegisterUserRequest Registration = new("buyer@example.com", "0912345678", "Password123!", "Buyer Test");

    [Fact]
    public async Task Register_ThenCapturedOtpVerification_ActivatesUserAndGrantsBuyerExactlyOnce()
    {
        using var factory = new AuthApplicationFactory();
        using var client = factory.CreateClient();
        var registration = await Register(client);
        factory.InitializationComplete.Should().BeTrue();
        factory.Mail.Deliveries.Should().ContainSingle();
        var delivered = factory.Mail.Deliveries.Single();
        delivered.Email.Should().Be(Registration.Email);
        delivered.Type.Should().Be(OtpType.EmailVerify);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Users.SingleAsync(user => user.Id == registration.UserId)).AccountStatus.Should().Be(AccountStatus.Unverified);
            (await db.UserRoles.CountAsync(role => role.UserId == registration.UserId)).Should().Be(0);
        }

        var response = await client.PostAsJsonAsync("/api/v1/auth/verify-otp", new VerifyOtpRequest(registration.UserId, delivered.Code));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.Mail.Deliveries.Should().ContainSingle();
        using var verifiedScope = factory.Services.CreateScope();
        var verifiedDb = verifiedScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await verifiedDb.Users.SingleAsync(user => user.Id == registration.UserId)).AccountStatus.Should().Be(AccountStatus.Active);
        var roles = await verifiedDb.UserRoles.Where(role => role.UserId == registration.UserId).Select(role => role.RoleId).ToListAsync();
        roles.Should().ContainSingle().Which.Should().Be(SystemRoleIds.Buyer);
        (await verifiedDb.UserOtps.SingleAsync(otp => otp.UserId == registration.UserId)).UsedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DuplicateEmailOrPhone_Returns409ProblemDetails(bool duplicateEmail)
    {
        using var factory = new AuthApplicationFactory();
        using var client = factory.CreateClient();
        await Register(client);
        var duplicate = duplicateEmail ? Registration with { Phone = "0987654321" } : Registration with { Email = "other@example.com" };
        await AssertProblem(await client.PostAsJsonAsync("/api/v1/auth/register", duplicate), 409, "Auth.DuplicateAccount");
        factory.Mail.Deliveries.Should().ContainSingle();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WrongOrExpiredOtp_Returns400ProblemDetails(bool expired)
    {
        using var factory = new AuthApplicationFactory();
        using var client = factory.CreateClient();
        var registration = await Register(client);
        var code = factory.Mail.Deliveries.Single().Code;
        if (expired) factory.Clock.UtcNow = factory.Clock.UtcNow.AddMinutes(6);
        else code = code == "000000" ? "111111" : "000000";
        await AssertProblem(await client.PostAsJsonAsync("/api/v1/auth/verify-otp", new VerifyOtpRequest(registration.UserId, code)), 400, "Auth.InvalidOtp");
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Users.SingleAsync(user => user.Id == registration.UserId)).AccountStatus.Should().Be(AccountStatus.Unverified);
        (await db.UserRoles.CountAsync(role => role.UserId == registration.UserId)).Should().Be(0);
    }

    [Fact]
    public async Task FiveWrongOtpAttempts_Return429ProblemDetailsWithRetryAfter()
    {
        using var factory = new AuthApplicationFactory();
        using var client = factory.CreateClient();
        var registration = await Register(client);
        var wrong = factory.Mail.Deliveries.Single().Code == "000000" ? "111111" : "000000";
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/verify-otp", new VerifyOtpRequest(registration.UserId, wrong));
            await AssertProblem(response, attempt < 5 ? 400 : 429, attempt < 5 ? "Auth.InvalidOtp" : "Auth.RateLimited");
            if (attempt == 5) response.Headers.RetryAfter!.Delta.Should().Be(TimeSpan.FromMinutes(5));
        }
    }

    [Fact]
    public async Task RegisterHttpRateLimit_Returns429ProblemDetails()
    {
        using var factory = new AuthApplicationFactory();
        using var client = factory.CreateClient();
        // Invalid DTOs consume the HTTP quota without creating users or sending emails.
        for (var attempt = 0; attempt < 20; attempt++)
            (await client.PostAsJsonAsync("/api/v1/auth/register", new { })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", Registration);
        await AssertProblem(response, 429, "Auth.RateLimited");
        response.Headers.RetryAfter.Should().NotBeNull();
        factory.Mail.Deliveries.Should().BeEmpty();
    }

    [Fact]
    public async Task Health_MissingBuyerRole_Returns503()
    {
        using var factory = new AuthApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Remove dependencies only in this disposable test database.
            await db.RolePermissions.Where(grant => grant.RoleId == SystemRoleIds.Buyer).ExecuteDeleteAsync();
            await db.Roles.Where(role => role.Id == SystemRoleIds.Buyer).ExecuteDeleteAsync();
        }
        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var roles = json.RootElement.GetProperty("checks").GetProperty("systemRoles");
        roles.GetProperty("status").GetString().Should().Be("Unhealthy");
        roles.GetProperty("data").GetProperty("Buyer").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Health_RedisFailure_Returns503WithoutLeakingExceptionDetails()
    {
        using var factory = new AuthApplicationFactory();
        using var client = factory.CreateClient();
        factory.RedisDatabase.Setup(db => db.PingAsync(It.IsAny<CommandFlags>()))
            .ThrowsAsync(new InvalidOperationException(AuthApplicationFactory.TestApiKey));
        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var text = await response.Content.ReadAsStringAsync();
        text.Should().NotContain(AuthApplicationFactory.TestApiKey);
        using var json = JsonDocument.Parse(text);
        json.RootElement.GetProperty("checks").GetProperty("redis").GetProperty("status").GetString().Should().Be("Unhealthy");
    }

    [Fact]
    public async Task Startup_DoesNotExposeClientUntilInitializationAndSeedingComplete()
    {
        using var factory = new AuthApplicationFactory(pauseInitialization: true);
        var creatingClient = Task.Run(() => factory.CreateClient());
        try
        {
            await factory.InitializationEntered.Task.WaitAsync(TimeSpan.FromSeconds(20));
            creatingClient.IsCompleted.Should().BeFalse();
            factory.InitializationComplete.Should().BeFalse();
        }
        finally { factory.ReleaseInitialization.TrySetResult(); }
        using var client = await creatingClient.WaitAsync(TimeSpan.FromSeconds(20));
        factory.InitializationComplete.Should().BeTrue();
        (await client.GetAsync("/health")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<RegisterUserResponse> Register(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", Registration);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterUserResponse>>();
        return envelope!.Data!;
    }

    private static async Task AssertProblem(HttpResponseMessage response, int status, string code)
    {
        ((int)response.StatusCode).Should().Be(status, await response.Content.ReadAsStringAsync());
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("status").GetInt32().Should().Be(status);
        json.RootElement.GetProperty("code").GetString().Should().Be(code);
    }
}
