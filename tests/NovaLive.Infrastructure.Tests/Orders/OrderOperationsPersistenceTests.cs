using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Orders.Commands.Checkout;
using NovaLive.Application.UseCases.Orders.Commands.CancelOrder;
using NovaLive.Application.UseCases.Orders.Commands.CancelSubOrder;
using NovaLive.Application.UseCases.Orders.Commands.ConfirmSubOrder;
using NovaLive.Application.UseCases.Orders.Queries.GetMyOrders;
using NovaLive.Application.UseCases.Orders.Queries.GetOrderDetail;
using NovaLive.Application.UseCases.Orders.Queries.GetSellerOrders;
using NovaLive.Application.UseCases.Orders.Queries.GetSellerOrderDetail;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Tests.Auth;
namespace NovaLive.Infrastructure.Tests.Orders;
public sealed class OrderOperationsPersistenceTests
{
    [Fact]
    public async Task BuyerQueriesAndCancel_PersistTimelineAndRelease_AndHideForeignOrders()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync(CheckoutPersistenceTests.Register);
        var buyer = await host.AddUser(); host.Authenticate(buyer);
        var request = await CheckoutPersistenceTests.Seed(host, buyer);
        var checkout = await host.Send(new CheckoutCommand(request, "buyer")); checkout.IsSuccess.Should().BeTrue();
        var id = checkout.Value!.ParentOrderId;
        var page = await host.Send(new GetMyOrdersQuery("Pending", host.Clock.UtcNow.AddDays(-1), host.Clock.UtcNow.AddDays(1), 1, 1));
        page.Value!.Total.Should().Be(1); page.Value.Items.Single().Id.Should().Be(id);
        (await host.Send(new GetMyOrdersQuery("Cancelled"))).Value!.Items.Should().BeEmpty();
        (await host.Send(new GetMyOrdersQuery(Page: 2, Size: 1))).Value!.Items.Should().BeEmpty();
        var detail = await host.Send(new GetOrderDetailQuery(id)); detail.Value!.SubOrders[0].Items.Should().ContainSingle();
        (await host.Send(new CancelOrderCommand(id, new("Changed mind")))).IsSuccess.Should().BeTrue();
        (await host.Send(new CancelOrderCommand(id, new("Repeat")))).Error.Code.Should().Be("Orders.AlreadyCancelled");
        var cancelled = await host.Send(new GetOrderDetailQuery(id));
        cancelled.Value!.PaymentStatus.Should().Be("Cancelled");
        cancelled.Value.SubOrders[0].Timeline.Select(h => h.ToStatus).Should().Equal("PendingPayment", "Cancelled");
        await host.WithDb(async db =>
        {
            (await db.Inventories.SingleAsync()).ReservedQty.Should().Be(0);
            (await db.InventoryHistories.CountAsync(h => h.ChangeType == InventoryChangeType.ReserveRelease)).Should().Be(1);
            (await db.Payments.SingleAsync()).Status.Should().Be(PaymentStatus.Expired);
        });
        var other = await host.AddUser("foreign@example.com", phone: "0900000001"); host.Authenticate(other);
        (await host.Send(new GetOrderDetailQuery(id))).Error.Code.Should().Be("Orders.OrderNotFound");
        (await host.Send(new GetMyOrdersQuery())).Value!.Items.Should().BeEmpty();
    }
    [Fact]
    public async Task SellerQueriesConfirmAndCancel_UseOwnedShop_AndRejectSpoofing()
    {
        await using var host = new AuthTestHost(); var shopContext = new Mock<ICurrentShop>(); Guid selectedShop = default;
        shopContext.SetupGet(s => s.ShopId).Returns(() => selectedShop);
        await host.InitializeAsync(s => { CheckoutPersistenceTests.Register(s); s.AddSingleton(shopContext.Object); });
        var buyer = await host.AddUser(); var seller = await host.AddUser("seller@example.com", phone: "0900000002");
        host.Authenticate(buyer); var request = await CheckoutPersistenceTests.Seed(host, buyer, ownerId: seller);
        var checkout = await host.Send(new CheckoutCommand(request with { PaymentMethod = "COD" }, "seller"));
        checkout.IsSuccess.Should().BeTrue(); var sub = checkout.Value!.SubOrders.Single(); selectedShop = sub.ShopId;
        (await host.Send(new CancelOrderCommand(checkout.Value.ParentOrderId, new("No")))).Error.Code.Should().Be("Orders.CannotCancelConfirmedOrder");
        await host.WithDb(async db =>
        {
            db.UserRoles.Add(new UserRole(seller, SystemRoleIds.Seller, host.Clock.UtcNow)); await db.SaveChangesAsync();
        });
        host.Authenticate(seller); host.CurrentUser.SetupGet(u => u.Roles).Returns(["Seller"]);
        var page = await host.Send(new GetSellerOrdersQuery("Confirmed", host.Clock.UtcNow.AddDays(-1), host.Clock.UtcNow.AddDays(1), 1, 1));
        page.IsSuccess.Should().BeTrue(); page.Value!.Items.Should().ContainSingle(s => s.Id == sub.Id);
        var detail = await host.Send(new GetSellerOrderDetailQuery(sub.Id)); detail.IsSuccess.Should().BeTrue();
        detail.Value!.Recipient.RecipientName.Should().Be("Recipient");
        var warehouse = detail.Value.WarehouseAddressId!.Value;
        (await host.Send(new ConfirmSubOrderCommand(sub.Id, new(warehouse)))).IsSuccess.Should().BeTrue();
        var confirmed = await host.Send(new GetSellerOrderDetailQuery(sub.Id));
        confirmed.Value!.Status.Should().Be("Confirmed"); confirmed.Value.Timeline.Should().HaveCount(3);
        selectedShop = Guid.NewGuid();
        (await host.Send(new GetSellerOrderDetailQuery(sub.Id))).Error.Code.Should().Be("Orders.SubOrderNotFound");
        (await host.Send(new CancelSubOrderCommand(sub.Id, new("Spoof")))).Error.Code.Should().Be("Orders.SubOrderNotFound");
        selectedShop = sub.ShopId;
        (await host.Send(new CancelSubOrderCommand(sub.Id, new("Warehouse issue")))).IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            (await db.SubOrders.SingleAsync()).Status.Should().Be(SubOrderStatus.Cancelled);
            (await db.ParentOrders.SingleAsync()).PaymentStatus.Should().Be(ParentOrderPaymentStatus.Cancelled);
            (await db.Inventories.SingleAsync()).ReservedQty.Should().Be(0);
        });
    }
}
