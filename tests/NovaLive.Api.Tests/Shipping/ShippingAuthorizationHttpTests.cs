using NovaLive.Application.Abstractions.Auth;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Api.Tests.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Api.Tests.Shipping;

[Collection("API host lifecycle")]
public sealed class ShippingAuthorizationHttpTests
{
    [Fact]
    public async Task AllFourRoutes_Enforce401And403()
    {
        using var app = new AuthApplicationFactory(); using var client = app.CreateClient();
        var id = Guid.NewGuid();
        var paths = new[] { $"/api/v1/seller/shipping/{id}/label",
            $"/api/v1/seller/shipping/{id}/tracking", $"/api/v1/orders/{id}/tracking" };
        async Task AssertStatus(HttpStatusCode expected)
        {
            foreach (var path in paths) (await client.GetAsync(path)).StatusCode.Should().Be(expected, path);
            (await client.PostAsJsonAsync("/api/v1/seller/shipping/create-order",
                new { subOrderId = id, provider = "GHN", serviceCode = "2", pickupAddressId = Guid.NewGuid() }))
                .StatusCode.Should().Be(expected);
        }
        await AssertStatus(HttpStatusCode.Unauthorized);
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = new User($"{Guid.NewGuid()}@example.com", "hash", "No permissions", null); user.Activate();
            db.Users.Add(user); await db.SaveChangesAsync();
            var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(user.Id, user.Email, ["Buyer"], null);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        }
        await AssertStatus(HttpStatusCode.Forbidden);
    }
}
