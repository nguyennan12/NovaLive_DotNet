using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Api.Tests.Auth;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Products;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using NovaLive.Domain.Rbac;
using NovaLive.Infrastructure.Persistence;
namespace NovaLive.Api.Tests.Carts;
[Collection("API host lifecycle")]
public sealed class CartHttpTests
{
    private static async Task<Guid> Authenticate(AuthApplicationFactory factory, HttpClient client, Guid? role = null)
    {
        using var scope = factory.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Mock.Get(scope.ServiceProvider.GetRequiredService<ICacheService>())
            .Setup(c => c.GetAsync<string[]>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((string[]?)null);
        var user = new User(Guid.NewGuid() + "@example.com", "hash", "Buyer", null); user.Activate();
        db.Users.Add(user); db.UserRoles.Add(new UserRole(user.Id, role ?? SystemRoleIds.Buyer, factory.Clock.UtcNow));
        await db.SaveChangesAsync();
        var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(user.Id, user.Email, ["Buyer"], null);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return user.Id;
    }
    private static async Task<T> Data<T>(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<ApiResponse<T>>())!.Data!;
    }
    private static Task<HttpResponseMessage> DeleteSelected(HttpClient client, params Guid[] ids) =>
        client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/v1/cart/items")
            { Content = JsonContent.Create(new RemoveCartItemsRequest(ids.ToList())) });
    [Fact]
    public async Task AllFiveRoutes_RejectAnonymousRequests()
    {
        using var factory = new AuthApplicationFactory(); using var client = factory.CreateClient();
        var id = Guid.NewGuid();
        var responses = new[] { await client.GetAsync("/api/v1/cart"), await client.PostAsJsonAsync("/api/v1/cart/items", new AddToCartRequest(id, 1)),
            await client.PutAsJsonAsync($"/api/v1/cart/items/{id}", new UpdateCartItemRequest(0)),
            await client.DeleteAsync($"/api/v1/cart/items/{id}"), await DeleteSelected(client, id) };
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task BuyerWorkflow_UsesCurrentPrices_DeniesForeignItems_AndValidatesInputs()
    {
        using var factory = new AuthApplicationFactory(); using var client = factory.CreateClient();
        var user = await Authenticate(factory, client);
        var empty = await Data<CartResponse>(await client.GetAsync("/api/v1/cart")); empty.ShopGroups.Should().BeEmpty();
        Guid skuId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Carts.CountAsync()).Should().Be(0);
            var shop = new Shop(Guid.NewGuid(), "Shop", "cart-shop"); shop.Approve();
            var spu = new Spu(shop.Id, Guid.NewGuid(), "Product", null, null, null, null);
            var sku = new Sku(spu.Id, shop.Id, "SKU", null, 100m, 90m); skuId = sku.Id;
            db.Shops.Add(shop); db.Spus.Add(spu); db.Skus.Add(sku);
            var stock = new NovaLive.Domain.Inventory.Inventory(sku.Id, shop.Id, 10); stock.Reserve(3); db.Inventories.Add(stock);
            await db.SaveChangesAsync();
        }
        var item = await Data<CartItemDto>(await client.PostAsJsonAsync("/api/v1/cart/items", new AddToCartRequest(skuId, 2)));
        item.Price.Should().Be(90m); item.AvailableStock.Should().Be(7);
        (await Data<CartItemDto>(await client.PostAsJsonAsync("/api/v1/cart/items", new AddToCartRequest(skuId, 1)))).Quantity.Should().Be(3);
        (await client.PostAsJsonAsync("/api/v1/cart/items", new AddToCartRequest(skuId, 5))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Skus.SingleAsync()).UpdatePriceAndDetails(80m, 100m, 200, true); await db.SaveChangesAsync();
            (await db.CartItems.SingleAsync()).UnitPrice.Should().Be(90m);
        }
        (await Data<CartResponse>(await client.GetAsync("/api/v1/cart"))).GrandTotal.Should().Be(240m);
        await Data<object>(await client.PutAsJsonAsync($"/api/v1/cart/items/{item.CartItemId}", new UpdateCartItemRequest(4)));
        var foreignUser = await Authenticate(factory, client);
        foreignUser.Should().NotBe(user);
        (await Data<CartResponse>(await client.GetAsync("/api/v1/cart"))).ShopGroups.Should().BeEmpty();
        (await client.PutAsJsonAsync($"/api/v1/cart/items/{item.CartItemId}", new UpdateCartItemRequest(0))).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.DeleteAsync($"/api/v1/cart/items/{item.CartItemId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Data<int>(await DeleteSelected(client, item.CartItemId, Guid.NewGuid()))).Should().Be(0);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.CartItems.SingleAsync()).Quantity.Should().Be(4);
            var owned = (await db.Carts.SingleAsync()).UserId; owned.Should().Be(user);
            var buyer = await db.Users.SingleAsync(u => u.Id == user);
            var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(user, buyer.Email, ["Buyer"], null);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        }
        (await client.PostAsJsonAsync("/api/v1/cart/items", new AddToCartRequest(Guid.Empty, 0))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PutAsJsonAsync($"/api/v1/cart/items/{item.CartItemId}", new UpdateCartItemRequest(-1))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await DeleteSelected(client)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Data<int>(await DeleteSelected(client, item.CartItemId, item.CartItemId, Guid.NewGuid()))).Should().Be(1);
        var again = await Data<CartItemDto>(await client.PostAsJsonAsync("/api/v1/cart/items", new AddToCartRequest(skuId, 1)));
        await Data<object>(await client.PutAsJsonAsync($"/api/v1/cart/items/{again.CartItemId}", new UpdateCartItemRequest(0)));
        var last = await Data<CartItemDto>(await client.PostAsJsonAsync("/api/v1/cart/items", new AddToCartRequest(skuId, 1)));
        await Data<object>(await client.DeleteAsync($"/api/v1/cart/items/{last.CartItemId}"));
        (await Data<CartResponse>(await client.GetAsync("/api/v1/cart"))).TotalItemsCount.Should().Be(0);
    }
    [Fact]
    public async Task AuthenticatedWithoutCartPermission_IsForbidden()
    {
        using var factory = new AuthApplicationFactory(); using var client = factory.CreateClient();
        await Authenticate(factory, client, Guid.NewGuid());
        (await client.GetAsync("/api/v1/cart")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    [Fact]
    public async Task ScalarAndOpenApi_ExposeAllFiveTypedEndpoints()
    {
        using var factory = new AuthApplicationFactory(environment: "Development"); using var client = factory.CreateClient();
        (await client.GetAsync("/scalar/v1")).StatusCode.Should().Be(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));
        var paths = json.RootElement.GetProperty("paths");
        var get = paths.GetProperty("/api/v1/cart").GetProperty("get");
        get.GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json")
            .GetProperty("schema").GetProperty("$ref").GetString().Should().Contain("CartResponse");
        paths.GetProperty("/api/v1/cart/items").GetProperty("post").GetProperty("requestBody").Should().NotBeNull();
        paths.GetProperty("/api/v1/cart/items").GetProperty("delete").GetProperty("requestBody").Should().NotBeNull();
        paths.GetProperty("/api/v1/cart/items/{cartItemId}").GetProperty("put").GetProperty("requestBody").Should().NotBeNull();
        paths.GetProperty("/api/v1/cart/items/{cartItemId}").GetProperty("delete").GetProperty("responses").GetProperty("200").Should().NotBeNull();
    }
}
