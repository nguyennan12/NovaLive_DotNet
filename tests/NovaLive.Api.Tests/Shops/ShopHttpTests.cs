using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Api.Tests.Auth;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;
namespace NovaLive.Api.Tests.Shops;

[Collection("API host lifecycle")]
public sealed class ShopHttpTests
{
    private static AuthApplicationFactory Factory(string environment = "Testing") => new(environment: environment, settings: new()
        { ["KycUrls:AllowedPrefixes:0"] = "https://cdn.example.com/kyc-docs/" });
    private static RegisterShopRequest Registration => new("Shop", "Description", "0912345678", "shop@example.com",
        "https://cdn.example.com/kyc-docs/front.png?signature=original", "https://cdn.example.com/kyc-docs/back.png", "123456789", "Bank");
    private static CreateShopAddressRequest Address => new("Warehouse", "Contact", "0912345678", "P", "D", "W", "Street", false, false, 1, 2, "0003");
    private static async Task<Guid> Authenticate(AuthApplicationFactory factory, HttpClient client)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Moq returns an empty array by default; Redis returns null for a cache miss.
        // Exercise the real RBAC repository/seeder instead of simulating a cached denial.
        Mock.Get(scope.ServiceProvider.GetRequiredService<ICacheService>())
            .Setup(cache => cache.GetAsync<string[]>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string[]?)null);
        var user = new User(Guid.NewGuid() + "@example.com", "hash", "Buyer", "0912345678"); user.Activate();
        db.Users.Add(user); db.UserRoles.Add(new UserRole(user.Id, SystemRoleIds.Buyer, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(user.Id, user.Email, ["Buyer"], null);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return user.Id;
    }
    private static async Task<T> Data<T>(HttpResponseMessage response, HttpStatusCode status = HttpStatusCode.OK)
    {
        response.StatusCode.Should().Be(status, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<ApiResponse<T>>())!.Data!;
    }

    [Fact]
    public async Task Register_PendingTrackingOnly_ThenApprovedSellerCanManage_EnforcesAc002()
    {
        using var factory = Factory(); using var client = factory.CreateClient(); var owner = await Authenticate(factory, client);
        var shop = await Data<ShopResponse>(await client.PostAsJsonAsync("/api/v1/shops/register", Registration), HttpStatusCode.Created);
        shop.Status.Should().Be("Pending"); shop.BankAccount.Should().Be("******6789");
        var duplicate = await client.PostAsJsonAsync("/api/v1/shops/register", Registration);
        duplicate.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var duplicateJson = JsonDocument.Parse(await duplicate.Content.ReadAsStringAsync());
        duplicateJson.RootElement.GetProperty("error").GetProperty("message").GetString().Should()
            .Be("Mỗi tài khoản chỉ được sở hữu tối đa 1 gian hàng");
        (await Data<ShopResponse>(await client.GetAsync("/api/v1/shops/me"))).VerificationStatus.Should().Be("Pending");
        {
            using var scope = factory.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var verification = await db.ShopVerifications.SingleAsync(v => v.ShopId == shop.Id);
        verification.IdCardFront.Should().Be(Registration.IdCardFront);
        verification.BankAccount.Should().Be(Registration.BankAccount);
        (await db.Users.SingleAsync(u => u.Id == owner)).IsSeller.Should().BeFalse();
        (await db.UserRoles.Where(r => r.UserId == owner).Select(r => r.RoleId).ToListAsync()).Should().Equal(SystemRoleIds.Buyer);
        (await db.ShopWallets.AnyAsync(w => w.ShopId == shop.Id)).Should().BeFalse();
        }
        await AssertSellerEndpointsForbidden(client);
        await GrantSeller(factory, client, owner);
        (await Data<ShopResponse>(await client.GetAsync("/api/v1/seller/shop/me"))).Id.Should().Be(shop.Id);
        var updated = await Data<ShopResponse>(await client.PutAsJsonAsync("/api/v1/seller/shop/me", new
            { shopName = "Changed", description = "Changed", phone = "0912345678", email = "shop@example.com",
              slug = "hacked", status = "Active", ownerId = Guid.NewGuid(), bankAccount = "hacked" }));
        updated.Slug.Should().Be(shop.Slug); updated.Status.Should().Be("Active"); updated.OwnerId.Should().Be(owner);
        var first = await Data<ShopAddressResponse>(await client.PostAsJsonAsync("/api/v1/seller/shop/addresses", Address), HttpStatusCode.Created);
        var second = await Data<ShopAddressResponse>(await client.PostAsJsonAsync("/api/v1/seller/shop/addresses",
            Address with { IsPrimary = true, IsReturn = true }), HttpStatusCode.Created);
        var list = await Data<List<ShopAddressResponse>>(await client.GetAsync("/api/v1/seller/shop/addresses"));
        list.Should().ContainSingle(a => a.IsPrimary).Which.Id.Should().Be(second.Id);
        list.Should().ContainSingle(a => a.IsReturn).Which.Id.Should().Be(second.Id);
        var promoted = await Data<ShopAddressResponse>(await client.PutAsJsonAsync($"/api/v1/seller/shop/addresses/{first.Id}",
            Address with { IsPrimary = true, IsReturn = true, DistrictId = 8 }));
        promoted.IsPrimary.Should().BeTrue(); promoted.IsReturn.Should().BeTrue(); promoted.DistrictId.Should().Be(8);
        (await client.DeleteAsync($"/api/v1/seller/shop/addresses/{first.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.DeleteAsync($"/api/v1/seller/shop/addresses/{second.Id}")).StatusCode.Should().Be(HttpStatusCode.BadRequest);

    }

    [Fact]
    public async Task ValidationAndAuthorization_RejectExternalKyc_AnonymousAndForeignWarehouse()
    {
        using var factory = Factory(); using var client = factory.CreateClient();
        (await client.GetAsync("/api/v1/shops/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.GetAsync($"/api/v1/shops/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var owner = await Authenticate(factory, client);
        (await client.PostAsJsonAsync("/api/v1/shops/register", Registration with { IdCardFront = "https://evil.example/id.png" }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await Data<ShopResponse>(await client.PostAsJsonAsync("/api/v1/shops/register", Registration), HttpStatusCode.Created);
        await GrantSeller(factory, client, owner);
        Guid foreignId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var foreign = new ShopAddress(Guid.NewGuid(), "Other", "Other", "0912345678", 1, "P", 2, "D", "3", "W", "Street");
            db.ShopAddresses.Add(foreign); await db.SaveChangesAsync(); foreignId = foreign.Id;
        }
        (await client.PutAsJsonAsync($"/api/v1/seller/shop/addresses/{foreignId}", Address)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.DeleteAsync($"/api/v1/seller/shop/addresses/{foreignId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Registration_WhenVerificationInsertFails_RollsBackShopAndOutbox()
    {
        using var factory = Factory(); using var client = factory.CreateClient(); await Authenticate(factory, client);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER reject_kyc BEFORE INSERT ON shop_verifications BEGIN SELECT RAISE(ABORT, 'test KYC insert failure'); END;");
        }
        (await client.PostAsJsonAsync("/api/v1/shops/register", Registration)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        using var check = factory.Services.CreateScope(); var saved = check.ServiceProvider.GetRequiredService<AppDbContext>();
        (await saved.Shops.CountAsync()).Should().Be(0);
        (await saved.ShopVerifications.CountAsync()).Should().Be(0);
        (await saved.OutboxMessages.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task PublicAndFollow_HidePendingAndKyc_AndAreIdempotent()
    {
        using var factory = Factory(); using var client = factory.CreateClient(); await Authenticate(factory, client);
        Guid shopId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); var shop = new Shop(Guid.NewGuid(), "Public", "public");
            db.Shops.Add(shop); await db.SaveChangesAsync(); shopId = shop.Id;
        }
        (await client.GetAsync($"/api/v1/shops/{shopId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.PostAsync($"/api/v1/shops/{shopId}/follow", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); (await db.Shops.SingleAsync()).Approve(); await db.SaveChangesAsync();
        }
        for (var i = 0; i < 2; i++) (await client.PostAsync($"/api/v1/shops/{shopId}/follow", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await Data<ShopDetailResponse>(await client.GetAsync($"/api/v1/shops/{shopId}"));
        detail.IsFollowing.Should().BeTrue(); detail.FollowersCount.Should().Be(1);
        for (var i = 0; i < 2; i++) (await client.DeleteAsync($"/api/v1/shops/{shopId}/follow")).StatusCode.Should().Be(HttpStatusCode.OK);
        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.GetAsync($"/api/v1/shops/{shopId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().NotContain("bankAccount").And.NotContain("idCardFront").And.NotContain("kycFrontUrl");
    }

    [Fact]
    public async Task OpenApi_DocumentsSeparatedPortalsAndNoLegacyAliases()
    {
        using var factory = Factory("Development"); using var client = factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()); var paths = json.RootElement.GetProperty("paths");
        foreach (var path in new[] { "/api/v1/shops/register", "/api/v1/shops/me", "/api/v1/shops/{shopId}/products", "/api/v1/seller/shop/me", "/api/v1/seller/shop/addresses" })
            paths.TryGetProperty(path, out _).Should().BeTrue(path);
        paths.GetProperty("/api/v1/shops/me").TryGetProperty("put", out _).Should().BeFalse();
        paths.TryGetProperty("/api/v1/shops/me/addresses", out _).Should().BeFalse();
        paths.TryGetProperty("/api/v1/shops/me/addresses/{id}", out _).Should().BeFalse();
        var schema = json.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("RegisterShopRequest").GetProperty("properties");
        schema.TryGetProperty("idCardFront", out _).Should().BeTrue();
        schema.TryGetProperty("idCardBack", out _).Should().BeTrue();
        schema.TryGetProperty("kycFrontUrl", out _).Should().BeFalse();
        schema.TryGetProperty("bankHolder", out _).Should().BeFalse();
        var responses = paths.GetProperty("/api/v1/shops/register").GetProperty("post").GetProperty("responses");
        foreach (var code in new[] { "201", "400", "401", "403", "404", "409" }) responses.TryGetProperty(code, out _).Should().BeTrue(code);
    }
    private static async Task GrantSeller(AuthApplicationFactory factory, HttpClient client, Guid owner, bool activateShop = true)
    {
        // Test fixture only: simulate already completed T14 approval; no approval endpoint is implemented here.
        using var scope = factory.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (activateShop) (await db.Shops.SingleAsync(s => s.OwnerId == owner)).Approve();
        db.UserRoles.Add(new UserRole(owner, SystemRoleIds.Seller, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        var user = await db.Users.SingleAsync(u => u.Id == owner);
        var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(owner, user.Email, ["Buyer", "Seller"], null);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
    }

    private static async Task AssertSellerEndpointsForbidden(HttpClient client)
    {
        var profile = new UpdateShopProfileRequest("Shop", "Description", null, null, "0912345678", "shop@example.com");
        var id = Guid.NewGuid();
        (await client.GetAsync("/api/v1/seller/shop/me")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PutAsJsonAsync("/api/v1/seller/shop/me", profile)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.GetAsync("/api/v1/seller/shop/addresses")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PostAsJsonAsync("/api/v1/seller/shop/addresses", Address)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PutAsJsonAsync($"/api/v1/seller/shop/addresses/{id}", Address)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.DeleteAsync($"/api/v1/seller/shop/addresses/{id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SellerClaimDoesNotBypassPendingStatus_OrManageOwnPermission()
    {
        using var factory = Factory(); using var client = factory.CreateClient(); var owner = await Authenticate(factory, client);
        await Data<ShopResponse>(await client.PostAsJsonAsync("/api/v1/shops/register", Registration), HttpStatusCode.Created);
        await GrantSeller(factory, client, owner, activateShop: false);
        await AssertSellerEndpointsForbidden(client);
        (await Data<ShopResponse>(await client.GetAsync("/api/v1/shops/me"))).Status.Should().Be("Pending");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Shops.SingleAsync(s => s.OwnerId == owner)).Approve();
            var permissionId = await db.Permissions.Where(p => p.Code == PermissionCodes.Shops.ManageOwn).Select(p => p.Id).SingleAsync();
            db.RolePermissions.RemoveRange(await db.RolePermissions.Where(p => p.RoleId == SystemRoleIds.Seller && p.PermissionId == permissionId).ToListAsync());
            await db.SaveChangesAsync();
        }
        await AssertSellerEndpointsForbidden(client);
    }

    [Fact]
    public async Task RemovedOwnerAliases_AreUnavailable()
    {
        using var factory = Factory(); using var client = factory.CreateClient(); await Authenticate(factory, client);
        (await client.PutAsJsonAsync("/api/v1/shops/me", new { })).StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        (await client.GetAsync("/api/v1/shops/me/addresses")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.PostAsJsonAsync("/api/v1/shops/me/addresses", Address)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.PutAsJsonAsync($"/api/v1/shops/me/addresses/{Guid.NewGuid()}", Address)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.DeleteAsync($"/api/v1/shops/me/addresses/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}
