using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Infrastructure.Persistence;
namespace NovaLive.Api.Tests.Returns;
[Collection("API host lifecycle")]
public sealed class SellerReturnsHttpTests
{
    private static async Task<Guid> Request(ReturnHttpTestContext c)
    {
        var response = await c.Client.PostAsJsonAsync("/api/v1/returns", c.Request);
        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<ApiResponse<OrderReturnResponse>>())!.Data!.Id;
    }
    [Fact]
    public async Task ApproveThenReceive_PersistsAtomicLedgersAndRetainsAddressSnapshot()
    {
        using var c = new ReturnHttpTestContext(); await c.SeedAsync(); var id = await Request(c);
        c.Authenticate(c.SellerId, true);
        var page = await c.Client.GetFromJsonAsync<ApiResponse<PagedResult<OrderReturnResponse>>>("/api/v1/seller/returns?status=Pending");
        page!.Data!.Items.Should().ContainSingle();
        var approved = await c.Client.PutAsJsonAsync($"/api/v1/seller/returns/{id}/approve", new SellerReturnResponseRequest("Ship here"));
        approved.StatusCode.Should().Be(HttpStatusCode.OK, await approved.Content.ReadAsStringAsync());
        (await approved.Content.ReadFromJsonAsync<ApiResponse<OrderReturnResponse>>())!.Data!.ReturnAddress!.DetailAddress.Should().Be("Return street");
        using (var scope = c.App.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); var address = await db.ShopAddresses.SingleAsync();
            address.UpdateInfo("Changed", "Seller", "0", 1, "P", 1, "D", "W", "W", "Different street"); await db.SaveChangesAsync();
        }
        c.Authenticate(c.BuyerId);
        var detail = await c.Client.GetFromJsonAsync<ApiResponse<OrderReturnResponse>>($"/api/v1/returns/{id}");
        detail!.Data!.ReturnAddress!.DetailAddress.Should().Be("Return street");
        c.Authenticate(c.SellerId, true);
        (await c.Client.PutAsync($"/api/v1/seller/returns/{id}/received", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.Client.PutAsync($"/api/v1/seller/returns/{id}/received", null)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var checkScope = c.App.Services.CreateScope(); var check = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await check.OrderReturns.SingleAsync()).Status.Should().Be(ReturnStatus.Completed);
        (await check.SubOrders.SingleAsync()).Status.Should().Be(SubOrderStatus.Returned);
        (await check.ShopWallets.SingleAsync()).Balance.Should().Be(900);
        (await check.Inventories.SingleAsync()).QtyOnHand.Should().Be(6);
        var money = await check.ShopWalletTransactions.SingleAsync(); money.Type.Should().Be(WalletTxType.OrderRefund);
        money.BalanceBefore.Should().Be(1000); money.BalanceAfter.Should().Be(900); money.Amount.Should().Be(-100);
        money.RefType.Should().Be("OrderReturn"); money.RefId.Should().Be(id);
        var stock = await check.InventoryHistories.SingleAsync(); stock.QtyBefore.Should().Be(5); stock.QtyAfter.Should().Be(6);
        stock.ChangeType.Should().Be(InventoryChangeType.ReturnIn); stock.RefId.Should().Be(id);
        (await check.OrderStatusHistories.CountAsync()).Should().Be(2);
    }
    [Fact]
    public async Task Reject_RequiresEvidenceAndLeavesSubOrderForT39()
    {
        using var c = new ReturnHttpTestContext(); await c.SeedAsync(); var id = await Request(c); c.Authenticate(c.SellerId, true);
        (await c.Client.PutAsJsonAsync($"/api/v1/seller/returns/{id}/reject", new RejectReturnRequest("Reason", []))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var response = await c.Client.PutAsJsonAsync($"/api/v1/seller/returns/{id}/reject", new RejectReturnRequest("Reason", ["https://example.com/seller.jpg"]));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<ApiResponse<OrderReturnResponse>>())!.Data!.SellerEvidenceUrls.Should().ContainSingle();
        (await c.Client.PutAsJsonAsync($"/api/v1/seller/returns/{id}/approve", new SellerReturnResponseRequest(null))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var scope = c.App.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.SubOrders.SingleAsync()).Status.Should().Be(SubOrderStatus.ReturnRequested);
        (await db.OrderReturns.SingleAsync()).Status.Should().Be(ReturnStatus.SellerRejected);
        (await db.ShopWalletTransactions.CountAsync()).Should().Be(0);
    }
    [Fact]
    public async Task InsufficientBalance_MapsTo400WithoutAnyChanges()
    {
        using var c = new ReturnHttpTestContext(); await c.SeedAsync(); var id = await Request(c); c.Authenticate(c.SellerId, true);
        (await c.Client.PutAsJsonAsync($"/api/v1/seller/returns/{id}/approve", new SellerReturnResponseRequest(null))).EnsureSuccessStatusCode();
        using (var scope = c.App.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); var wallet = await db.ShopWallets.SingleAsync();
            wallet.DebitRefund(1000, c.Factory.Clock.UtcNow); await db.SaveChangesAsync();
        }
        var result = await c.Client.PutAsync($"/api/v1/seller/returns/{id}/received", null);
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest); (await result.Content.ReadAsStringAsync()).Should().Contain("Returns.InsufficientShopBalance");
        using var checkScope = c.App.Services.CreateScope(); var check = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await check.OrderReturns.SingleAsync()).Status.Should().Be(ReturnStatus.SellerApproved);
        (await check.SubOrders.SingleAsync()).Status.Should().Be(SubOrderStatus.ReturnRequested);
        (await check.Inventories.SingleAsync()).QtyOnHand.Should().Be(5);
        (await check.ShopWalletTransactions.CountAsync()).Should().Be(0); (await check.InventoryHistories.CountAsync()).Should().Be(0);
    }
    [Fact]
    public async Task LedgerFailure_RollsBackWalletStockReturnAndHistory()
    {
        var ledger = new Mock<IInventoryHistoryRepository>();
        ledger.Setup(h => h.AddAsync(It.IsAny<InventoryHistory>(), It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("Injected ledger failure"));
        using var c = new ReturnHttpTestContext(s => { s.RemoveAll<IInventoryHistoryRepository>(); s.AddSingleton(ledger.Object); });
        await c.SeedAsync(); var id = await Request(c); c.Authenticate(c.SellerId, true);
        (await c.Client.PutAsJsonAsync($"/api/v1/seller/returns/{id}/approve", new SellerReturnResponseRequest(null))).EnsureSuccessStatusCode();
        (await c.Client.PutAsync($"/api/v1/seller/returns/{id}/received", null)).StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        using var scope = c.App.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.OrderReturns.SingleAsync()).Status.Should().Be(ReturnStatus.SellerApproved);
        (await db.SubOrders.SingleAsync()).Status.Should().Be(SubOrderStatus.ReturnRequested);
        (await db.ShopWallets.SingleAsync()).Balance.Should().Be(1000); (await db.Inventories.SingleAsync()).QtyOnHand.Should().Be(5);
        (await db.ShopWalletTransactions.CountAsync()).Should().Be(0); (await db.InventoryHistories.CountAsync()).Should().Be(0);
        (await db.OrderStatusHistories.CountAsync()).Should().Be(1);
    }
    [Fact]
    public async Task SellerEndpoints_EnforceAuthenticationPermissionAndOwnedShopContext()
    {
        using var c = new ReturnHttpTestContext(); await c.SeedAsync(); var id = await Request(c);
        (await c.Client.GetAsync("/api/v1/seller/returns")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        c.Client.DefaultRequestHeaders.Authorization = null;
        (await c.Client.GetAsync("/api/v1/seller/returns")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        c.Authenticate(c.SellerId, true); c.Client.DefaultRequestHeaders.Remove("X-Shop-Id");
        c.Client.DefaultRequestHeaders.Add("X-Shop-Id", Guid.NewGuid().ToString());
        (await c.Client.GetAsync("/api/v1/seller/returns")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await c.Client.PutAsJsonAsync($"/api/v1/seller/returns/{id}/approve", new SellerReturnResponseRequest(null))).StatusCode.Should().Be(HttpStatusCode.NotFound);
        c.Client.DefaultRequestHeaders.Remove("X-Shop-Id"); c.Client.DefaultRequestHeaders.Add("X-Shop-Id", c.ShopId.ToString());
        (await c.Client.GetAsync("/api/v1/seller/returns?status=invalid&page=0")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await c.Client.PutAsync($"/api/v1/seller/returns/{Guid.NewGuid()}/received", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
