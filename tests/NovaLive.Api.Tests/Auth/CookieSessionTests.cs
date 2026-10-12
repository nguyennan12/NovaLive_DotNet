using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Api.Tests.Auth;

[Collection("API host lifecycle")]
public sealed class CookieSessionTests
{
    private const string Password = "Password123!";
    private const string Email = "cookie@example.com";

    private static HttpClient Client(AuthApplicationFactory factory) => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"), HandleCookies = false, AllowAutoRedirect = false
    });

    private static IList<SetCookieHeaderValue> Cookies(HttpResponseMessage response) =>
        SetCookieHeaderValue.ParseList(response.Headers.GetValues("Set-Cookie").ToList());
    private static string Value(HttpResponseMessage response, string name) => Cookies(response).Single(c => c.Name == name).Value.ToString();
    private static string CookieHeader(HttpResponseMessage response) => string.Join("; ", Cookies(response).Select(c => $"{c.Name}={c.Value}"));

    private static async Task<HttpResponseMessage> Verify(AuthApplicationFactory factory, HttpClient client)
    {
        var registration = await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterUserRequest(Email, "0912345678", Password, "Cookie Test"));
        registration.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await registration.Content.ReadAsStringAsync());
        var id = body.RootElement.GetProperty("data").GetProperty("userId").GetGuid();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/verify-otp")
        {
            Content = JsonContent.Create(new VerifyOtpRequest(id, factory.Mail.Deliveries.Single().Code))
        };
        request.Headers.UserAgent.ParseAdd("CookieIntegration/1.0");
        var verified = await client.SendAsync(request);
        verified.StatusCode.Should().Be(HttpStatusCode.OK, await verified.Content.ReadAsStringAsync());
        return verified;
    }

    private static Task<HttpResponseMessage> Send(HttpClient client, string path, string cookie, bool csrf = true, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/" + path);
        request.Headers.Add("Cookie", cookie);
        if (csrf) request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        if (body is not null) request.Content = JsonContent.Create(body);
        return client.SendAsync(request);
    }

    private static async Task AssertSessionBody(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        var text = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(text);
        json.RootElement.GetProperty("data").EnumerateObject().Select(p => p.Name)
            .Should().BeEquivalentTo("user", "expiresAt");
        text.Should().NotContain("accessToken").And.NotContain("refreshToken");
        foreach (var cookie in Cookies(response)) text.Should().NotContain(cookie.Value.ToString());
    }

    [Theory]
    [InlineData("Development", "Lax", false, false)]
    [InlineData("Development", "Lax", true, true)]
    [InlineData("Production", "Lax", false, true)]
    [InlineData("Staging", "Strict", false, true)]
    [InlineData("Development", "None", false, true)]
    public async Task Verify_IssuesOnlyCookies_WithConfiguredAttributesAndBuyerClaims(string environment, string sameSite, bool secure, bool expectedSecure)
    {
        using var factory = new AuthApplicationFactory(environment: environment, settings: new()
        {
            ["Cookies:AccessTokenName"] = "test_at", ["Cookies:RefreshTokenName"] = "test_rt",
            ["Cookies:Secure"] = secure.ToString(), ["Cookies:SameSite"] = sameSite, ["Cookies:Domain"] = "localhost"
        });
        using var client = Client(factory);
        var verified = await Verify(factory, client);
        await AssertSessionBody(verified);
        var cookies = Cookies(verified);
        cookies.Should().HaveCount(2);
        foreach (var cookie in cookies)
        {
            cookie.HttpOnly.Should().BeTrue();
            cookie.Secure.Should().Be(expectedSecure);
            cookie.SameSite.ToString().Should().Be(sameSite);
            cookie.Domain.ToString().Should().Be("localhost");
        }
        var access = cookies.Single(c => c.Name == "test_at");
        access.Path.ToString().Should().Be("/");
        access.MaxAge.Should().Be(TimeSpan.FromMinutes(15));
        var refresh = cookies.Single(c => c.Name == "test_rt");
        refresh.Path.ToString().Should().Be("/api/v1/auth");
        refresh.MaxAge.Should().Be(TimeSpan.FromDays(30));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(access.Value.ToString());
        jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value).Should().Contain("Buyer");
        using var scope = factory.Services.CreateScope();
        var token = await scope.ServiceProvider.GetRequiredService<AppDbContext>().RefreshTokens.SingleAsync();
        token.TokenHash.Should().Be(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refresh.Value.ToString()))));
        token.DeviceInfo.Should().Be("CookieIntegration/1.0");
        token.ExpiresAt.Should().Be(token.CreatedAt.AddDays(30));
    }

    [Fact]
    public async Task CookieAuthentication_HeaderPrecedence_AndLoginResponse_Work()
    {
        using var factory = new AuthApplicationFactory();
        using var client = Client(factory);
        var verified = await Verify(factory, client);
        Cookies(verified).Should().OnlyContain(cookie => !cookie.Domain.HasValue);
        using var me = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        me.Headers.Add("Cookie", CookieHeader(verified));
        (await client.SendAsync(me)).StatusCode.Should().Be(HttpStatusCode.OK);

        using var header = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        header.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Value(verified, "nova_at"));
        header.Headers.Add("Cookie", "nova_at=invalid");
        (await client.SendAsync(header)).StatusCode.Should().Be(HttpStatusCode.OK);

        using var invalidHeader = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        invalidHeader.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid");
        invalidHeader.Headers.Add("Cookie", CookieHeader(verified));
        (await client.SendAsync(invalidHeader)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(Email, Password));
        await AssertSessionBody(login);
        Value(login, "nova_at").Should().NotBe(Value(verified, "nova_at"));
        Value(login, "nova_rt").Should().NotBe(Value(verified, "nova_rt"));
    }

    [Fact]
    public async Task Refresh_RequiresCookie_EvenWhenLegacyTokenBodyIsProvided()
    {
        using var factory = new AuthApplicationFactory();
        using var client = Client(factory);
        var verified = await Verify(factory, client);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh")
        {
            Content = JsonContent.Create(new { refreshToken = Value(verified, "nova_rt") })
        };
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        AssertDeletedCookies(response);
        using var scope = factory.Services.CreateScope();
        (await scope.ServiceProvider.GetRequiredService<AppDbContext>().RefreshTokens.SingleAsync()).RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task Refresh_RotatesCookie_ReuseCommitsRevocationOfEverySessionAndClearsCookies()
    {
        using var factory = new AuthApplicationFactory();
        using var client = Client(factory);
        var verified = await Verify(factory, client);
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(Email, Password));
        var rotated = await Send(client, "refresh", CookieHeader(verified));
        await AssertSessionBody(rotated);
        Value(rotated, "nova_rt").Should().NotBe(Value(verified, "nova_rt"));
        using (var scope = factory.Services.CreateScope())
        {
            var tokens = await scope.ServiceProvider.GetRequiredService<AppDbContext>().RefreshTokens.ToListAsync();
            tokens.Should().HaveCount(3);
            tokens.Count(t => t.RevokedAt != null).Should().Be(1);
        }
        var reuse = await Send(client, "refresh", CookieHeader(verified));
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        AssertDeletedCookies(reuse);
        using (var scope = factory.Services.CreateScope())
            (await scope.ServiceProvider.GetRequiredService<AppDbContext>().RefreshTokens.ToListAsync())
                .Should().HaveCount(3).And.OnlyContain(t => t.RevokedAt != null);
        (await Send(client, "refresh", CookieHeader(rotated))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Send(client, "refresh", CookieHeader(login))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesAllRefreshSessions_BlacklistsCurrentJwt_AndDeletesCookies()
    {
        using var factory = new AuthApplicationFactory(settings: new() { ["Cookies:Domain"] = "localhost", ["Cookies:SameSite"] = "Strict" });
        using var client = Client(factory);
        var verified = await Verify(factory, client);
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(Email, Password));
        var loggedOut = await Send(client, "logout", CookieHeader(login));
        loggedOut.StatusCode.Should().Be(HttpStatusCode.OK);
        AssertDeletedCookies(loggedOut);
        foreach (var deleted in Cookies(loggedOut))
        {
            var issued = Cookies(login).Single(c => c.Name == deleted.Name);
            deleted.Domain.Should().Be(issued.Domain);
            deleted.Path.Should().Be(issued.Path);
            deleted.SameSite.Should().Be(issued.SameSite);
            deleted.Secure.Should().Be(issued.Secure);
        }
        using var scope = factory.Services.CreateScope();
        (await scope.ServiceProvider.GetRequiredService<AppDbContext>().RefreshTokens.ToListAsync())
            .Should().HaveCount(2).And.OnlyContain(t => t.RevokedAt != null);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(Value(login, "nova_at"));
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var ttl = await cache.GetTimeToLiveAsync("Blacklist:" + jwt.Id);
        ttl.Should().BeCloseTo(TimeSpan.FromMinutes(15), TimeSpan.FromSeconds(1));
        using var me = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        me.Headers.Add("Cookie", CookieHeader(login));
        (await client.SendAsync(me)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Send(client, "refresh", "nova_rt=" + Value(verified, "nova_rt"))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("refresh")]
    [InlineData("logout")]
    [InlineData("change-password")]
    public async Task CookieMutation_RequiresCsrfHeaderBeforeChangingState(string endpoint)
    {
        using var factory = new AuthApplicationFactory();
        using var client = Client(factory);
        var verified = await Verify(factory, client);
        var response = await Send(client, endpoint, CookieHeader(verified), csrf: false,
            body: endpoint == "change-password" ? new ChangePasswordRequest(Password, "ChangedPassword123!") : null);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        (await response.Content.ReadAsStringAsync()).Should().Contain("Auth.CsrfRejected");
        using var scope = factory.Services.CreateScope();
        (await scope.ServiceProvider.GetRequiredService<AppDbContext>().RefreshTokens.SingleAsync()).RevokedAt.Should().BeNull();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(Email, Password));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cors_AllowsOnlyConfiguredOrigins_WithCredentialsAndCsrfPreflight()
    {
        using var factory = new AuthApplicationFactory(settings: new() { ["Cors:AllowedOrigins:0"] = "https://frontend.example" });
        using var client = Client(factory);
        foreach (var origin in new[] { "https://frontend.example", "https://evil.example" })
        {
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/refresh");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "POST");
            request.Headers.Add("Access-Control-Request-Headers", "X-Requested-With,Content-Type");
            var response = await client.SendAsync(request);
            if (origin == "https://frontend.example")
            {
                response.Headers.GetValues("Access-Control-Allow-Origin").Should().ContainSingle().Which.Should().Be(origin);
                response.Headers.GetValues("Access-Control-Allow-Credentials").Should().Contain("true");
                string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers")).Should().ContainEquivalentOf("X-Requested-With");
            }
            else response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
        }
    }

    private static void AssertDeletedCookies(HttpResponseMessage response)
    {
        var cookies = Cookies(response);
        cookies.Should().HaveCount(2);
        cookies.Single(c => c.Name == "nova_at").Path.ToString().Should().Be("/");
        cookies.Single(c => c.Name == "nova_rt").Path.ToString().Should().Be("/api/v1/auth");
        foreach (var cookie in cookies)
        {
            cookie.Value.ToString().Should().BeEmpty();
            cookie.Expires.Should().BeBefore(DateTimeOffset.UtcNow);
            cookie.HttpOnly.Should().BeTrue();
        }
    }
}
