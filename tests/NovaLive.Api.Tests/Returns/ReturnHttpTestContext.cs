using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Api.Tests.Auth;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Products;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;
using Moq;
using NovaLive.Application.Abstractions.Services;
namespace NovaLive.Api.Tests.Returns;
internal sealed class ReturnHttpTestContext : IDisposable
{
    public AuthApplicationFactory Factory { get; } = new();
    public WebApplicationFactory<Program> App { get; }
    public HttpClient Client { get; }
    public Guid BuyerId { get; private set; }
    public Guid SellerId { get; private set; }
    public Guid SubId { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid ShopId { get; private set; }
    public CreateReturnRequest Request => new(SubId, "WrongItem", [new(ItemId, 1, null)], ["https://example.com/evidence.jpg"]);
    public ReturnHttpTestContext(Action<IServiceCollection>? configure = null)
    {
        App = configure == null ? Factory : Factory.WithWebHostBuilder(b => b.ConfigureTestServices(configure));
        Client = App.CreateClient();
    }
    public async Task SeedAsync(decimal productDiscount = 0)
    {
        using var scope = App.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Mock.Get(scope.ServiceProvider.GetRequiredService<ICacheService>())
            .Setup(cache => cache.GetAsync<string[]>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string[]?)null);
        var buyer = new User("buyer-returns@example.com", "hash", "Buyer", null); buyer.Activate();
        var seller = new User("seller-returns@example.com", "hash", "Seller", null); seller.Activate();
        var shop = new Shop(seller.Id, "Returns shop", "returns-shop"); shop.Approve();
        var warehouse = new ShopAddress(shop.Id, "Return warehouse", "Seller", "0900000000", 1, "P", 1, "D", "W", "Ward", "Return street"); warehouse.SetReturn(true);
        var spu = new Spu(shop.Id, Guid.NewGuid(), "Product", null, null, null, null);
        var sku = new Sku(spu.Id, shop.Id, "RET-SKU", null, 100, 100, 100);
        var parent = ParentOrder.Create(buyer.Id, "RETURN-PARENT", "{}", 200, 10, productDiscount, 210 - productDiscount, null, Factory.Clock.UtcNow.AddDays(-2));
        var sub = SubOrder.Create(parent.Id, "RETURN-SUB", warehouse.Id,
            new ShopCalculationResult(shop.Id, 200, 10, 0, productDiscount, 0, 200 - productDiscount, 210 - productDiscount, []), true, Factory.Clock.UtcNow.AddDays(-2));
        sub.MarkAsShipping(Factory.Clock.UtcNow.AddDays(-2)); sub.MarkAsDelivered(Factory.Clock.UtcNow.AddDays(-1));
        var item = OrderItem.Create(sub.Id, """{"spuName":"Product","skuCode":"RET-SKU"}""", 100,
            new(sku.Id, spu.Id, 2, 100, 200, 0, productDiscount, productDiscount, 200 - productDiscount));
        var wallet = new ShopWallet(shop.Id); wallet.Credit(1000, Factory.Clock.UtcNow);
        db.Users.AddRange(buyer, seller); db.UserRoles.AddRange(new UserRole(buyer.Id, SystemRoleIds.Buyer, Factory.Clock.UtcNow),
            new UserRole(seller.Id, SystemRoleIds.Buyer, Factory.Clock.UtcNow),
            new UserRole(seller.Id, SystemRoleIds.Seller, Factory.Clock.UtcNow));
        db.Shops.Add(shop); db.ShopAddresses.Add(warehouse); db.Spus.Add(spu); db.Skus.Add(sku);
        db.Inventories.Add(new NovaLive.Domain.Inventory.Inventory(sku.Id, shop.Id, 5)); db.ShopWallets.Add(wallet);
        db.ParentOrders.Add(parent); db.SubOrders.Add(sub); db.OrderItems.Add(item); await db.SaveChangesAsync();
        BuyerId = buyer.Id; SellerId = seller.Id; SubId = sub.Id; ItemId = item.Id; ShopId = shop.Id;
        Authenticate(BuyerId);
    }
    public void Authenticate(Guid id, bool seller = false)
    {
        using var scope = App.Services.CreateScope();
        var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(id, "user@example.com", seller ? ["Seller"] : ["Buyer"], seller ? ShopId : null);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        Client.DefaultRequestHeaders.Remove("X-Shop-Id");
        if (seller) Client.DefaultRequestHeaders.Add("X-Shop-Id", ShopId.ToString());
    }
    public void Dispose() { Client.Dispose(); if (App != Factory) App.Dispose(); Factory.Dispose(); }
}
