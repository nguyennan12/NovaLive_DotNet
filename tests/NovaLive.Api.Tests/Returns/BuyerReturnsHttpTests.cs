using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Api.Controllers.Returns;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.Persistence;
namespace NovaLive.Api.Tests.Returns;
[Collection("API host lifecycle")]
public sealed class BuyerReturnsHttpTests
{
    [Fact]
    public async Task RequestAndRead_PersistReturnItemsHistoryAndEnforceOwnership()
    {
        using var c = new ReturnHttpTestContext(); await c.SeedAsync();
        var response = await c.Client.PostAsJsonAsync("/api/v1/returns", c.Request);
        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        var created = (await response.Content.ReadFromJsonAsync<ApiResponse<OrderReturnResponse>>())!.Data!;
        created.RequestedRefundAmount.Should().Be(100);
        (await c.Client.GetAsync($"/api/v1/returns/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await c.Client.GetFromJsonAsync<ApiResponse<PagedResult<OrderReturnResponse>>>("/api/v1/returns?status=Pending&page=1&size=5");
        page!.Data!.Items.Should().ContainSingle();
        (await c.Client.PostAsJsonAsync("/api/v1/returns", c.Request)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using (var scope = c.App.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.SubOrders.SingleAsync()).Status.Should().Be(SubOrderStatus.ReturnRequested);
            (await db.OrderReturns.CountAsync()).Should().Be(1); (await db.OrderReturnItems.CountAsync()).Should().Be(1);
            (await db.OrderStatusHistories.SingleAsync()).ChangedByRole.Should().Be(OrderRole.Buyer);
        }
        c.Authenticate(c.SellerId); // Valid user with Buyer token still cannot read another buyer's return.
        (await c.Client.GetAsync($"/api/v1/returns/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await c.Client.PostAsJsonAsync("/api/v1/returns", c.Request)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task AuthenticationPermissionAndValidation_MapToHttpStatuses()
    {
        using var c = new ReturnHttpTestContext(); await c.SeedAsync();
        c.Client.DefaultRequestHeaders.Authorization = null;
        (await c.Client.GetAsync("/api/v1/returns")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        c.Authenticate(c.SellerId, true);
        (await c.Client.GetAsync("/api/v1/returns")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        c.Authenticate(c.BuyerId);
        (await c.Client.PostAsJsonAsync("/api/v1/returns", c.Request with { EvidenceUrls = [] })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await c.Client.GetAsync("/api/v1/returns?status=invalid&page=0")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        typeof(ReturnsController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Should().ContainSingle();
    }
}
