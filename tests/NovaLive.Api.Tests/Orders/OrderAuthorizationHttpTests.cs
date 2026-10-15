using NovaLive.Application.Abstractions.Auth;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Api.Tests.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;
namespace NovaLive.Api.Tests.Orders;
[Collection("API host lifecycle")]
public sealed class OrderAuthorizationHttpTests
{
    [Fact]
    public async Task AllOrderRoutes_Enforce401And403BeforeReadingResources()
    {
        using var app = new AuthApplicationFactory(); using var client = app.CreateClient();
        var id = Guid.NewGuid();
        var routes = new[] { ("GET", "/api/v1/orders"), ("GET", $"/api/v1/orders/{id}"),
            ("POST", "/api/v1/orders/calculate-checkout"), ("POST", "/api/v1/orders/checkout"), ("POST", $"/api/v1/orders/{id}/cancel"),
            ("GET", "/api/v1/seller/orders"), ("GET", $"/api/v1/seller/orders/{id}"),
            ("PUT", $"/api/v1/seller/orders/{id}/confirm"), ("POST", $"/api/v1/seller/orders/{id}/cancel") };
        async Task<HttpStatusCode> Send(string method, string path)
        {
            using var request = new HttpRequestMessage(new HttpMethod(method), path);
            request.Headers.Add("Idempotency-Key", "http-key");
            if (method != "GET") request.Content = JsonContent.Create(new
            {
                cartItemIds = new[] { Guid.NewGuid() }, shippingAddressId = Guid.NewGuid(),
                shippingProviders = new[] { new { shopId = Guid.NewGuid(), provider = "GHN", serviceCode = "2" } },
                paymentMethod = "MoMo", reason = "Issue", warehouseAddressId = Guid.NewGuid()
            });
            using var response = await client.SendAsync(request); return response.StatusCode;
        }
        foreach (var (method, path) in routes) (await Send(method, path)).Should().Be(HttpStatusCode.Unauthorized, path);
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = new User($"{Guid.NewGuid()}@example.com", "hash", "No permission", null); user.Activate(); db.Users.Add(user);
            await db.SaveChangesAsync();
            var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(user.Id, user.Email, ["Buyer"], null);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        }
        foreach (var (method, path) in routes) (await Send(method, path)).Should().Be(HttpStatusCode.Forbidden, path);
    }
    [Fact]
    public async Task AuthorizedBuyerAndSeller_ReachValidatorsAndCheckoutRequiresIdempotencyHeader()
    {
        using var app = new AuthApplicationFactory(); using var client = app.CreateClient();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = new User($"{Guid.NewGuid()}@example.com", "hash", "Seller", null); user.Activate(); db.Users.Add(user);
            db.UserRoles.Add(new UserRole(user.Id, SystemRoleIds.Seller, DateTimeOffset.UtcNow));
            db.UserRoles.Add(new UserRole(user.Id, SystemRoleIds.Buyer, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync();
            var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(user.Id, user.Email, ["Buyer", "Seller"], Guid.NewGuid());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        }
        (await client.GetAsync("/api/v1/orders?page=0&size=101")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.GetAsync("/api/v1/seller/orders?status=Wrong")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PostAsJsonAsync("/api/v1/orders/checkout", new
        {
            cartItemIds = new[] { Guid.NewGuid() }, shippingAddressId = Guid.NewGuid(),
            shippingProviders = new[] { new { shopId = Guid.NewGuid(), provider = "GHN", serviceCode = "2" } }, paymentMethod = "MoMo"
        })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PostAsJsonAsync($"/api/v1/orders/{Guid.NewGuid()}/cancel", new { reason = "" }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PutAsJsonAsync($"/api/v1/seller/orders/{Guid.NewGuid()}/confirm", new { warehouseAddressId = Guid.Empty }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
