using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Application.UseCases.Orders.Commands.Checkout;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Carts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Products;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Infrastructure.Tests.Auth;
namespace NovaLive.Infrastructure.Tests.Orders;
public sealed class CheckoutPersistenceTests
{
    internal static void Register(IServiceCollection s)
    {
        s.AddScoped<ICartRepository, CartRepository>(); s.AddScoped<IShopRepository, ShopRepository>();
        s.AddScoped<IUserAddressRepository, UserAddressRepository>(); s.AddScoped<IShopAddressRepository, ShopAddressRepository>();
        s.AddScoped<IDiscountRepository, DiscountRepository>(); s.AddScoped<IParentOrderRepository, ParentOrderRepository>();
        s.AddScoped<ISubOrderRepository, SubOrderRepository>(); s.AddScoped<IOrderItemRepository, OrderItemRepository>();
        s.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>(); s.AddScoped<IPaymentRepository, PaymentRepository>();
        s.AddScoped<IOutboxRepository, OutboxRepository>();
        s.AddScoped<IFlashSaleRepository, FlashSaleRepository>();
        var shipping = new Mock<IShippingProvider>(); shipping.SetupGet(p => p.ProviderCode).Returns("GHN");
        shipping.Setup(p => p.CalculateFeeAsync(It.IsAny<ShippingFeeCalculationModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ShippingFeeQuote("GHN", "2", 30)));
        s.AddSingleton(shipping.Object);
    }
    internal static async Task<CheckoutRequest> Seed(AuthTestHost host, Guid buyer, Guid? skuId = null, Guid? ownerId = null)
    {
        CheckoutRequest request = null!;
        await host.WithDb(async db =>
        {
            Sku sku;
            if (skuId is null)
            {
                var shop = new Shop(ownerId ?? Guid.NewGuid(), "Shop", Guid.NewGuid().ToString()); shop.Approve(); db.Shops.Add(shop);
                var spu = new Spu(shop.Id, Guid.NewGuid(), "Product", null, null, "image", null); db.Spus.Add(spu);
                sku = new Sku(spu.Id, shop.Id, "SKU", "{}", 120, 100); db.Skus.Add(sku);
                db.Inventories.Add(new NovaLive.Domain.Inventory.Inventory(sku.Id, shop.Id, 1));
                var warehouse = new ShopAddress(shop.Id, "Primary", "Seller", "0", 1, "P", 100, "D", "W", "W", "Street");
                warehouse.SetPrimary(true); db.ShopAddresses.Add(warehouse);
                db.Discounts.Add(new Discount(null, "TEN", "Ten", DiscountType.FixedCart, 10, 0, null, 1, 1,
                    DiscountAppliesTo.AllProducts, null, host.Clock.UtcNow.AddDays(-1), host.Clock.UtcNow.AddDays(1), true, true));
            }
            else sku = await db.Skus.SingleAsync(s => s.Id == skuId);
            var address = new UserAddress(buyer, "Recipient", "0", 1, "P", 200, "D", "W", "W", "Street"); db.UserAddresses.Add(address);
            var cart = new Cart(buyer, host.Clock.UtcNow); db.Carts.Add(cart);
            var item = new CartItem(Guid.NewGuid(), cart.Id, sku.Id, sku.ShopId, 1, 1, host.Clock.UtcNow); db.CartItems.Add(item);
            await db.SaveChangesAsync();
            request = new([item.Id], address.Id, [new(sku.ShopId, "GHN", "2")], null, "TEN", null, "MoMo", null);
        });
        return request;
    }
    [Fact]
    public async Task FailureAtOutbox_RollsBackEveryTableAndVoucherIncrement()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync(s =>
        {
            Register(s);
            var outbox = new Mock<IOutboxRepository>();
            outbox.Setup(o => o.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("Injected outbox failure"));
            s.AddScoped(_ => outbox.Object);
        });
        var buyer = await host.AddUser(); host.Authenticate(buyer); var request = await Seed(host, buyer);
        await FluentActions.Awaiting(() => host.Send(new CheckoutCommand(request, "rollback"))).Should().ThrowAsync<IOException>();
        await host.WithDb(async db =>
        {
            (await db.ParentOrders.CountAsync()).Should().Be(0); (await db.SubOrders.CountAsync()).Should().Be(0);
            (await db.OrderItems.CountAsync()).Should().Be(0); (await db.Payments.CountAsync()).Should().Be(0);
            (await db.InventoryHistories.CountAsync()).Should().Be(0); (await db.OrderStatusHistories.CountAsync()).Should().Be(0);
            (await db.DiscountUsages.CountAsync()).Should().Be(0); (await db.OutboxMessages.CountAsync()).Should().Be(0);
            (await db.Inventories.SingleAsync()).ReservedQty.Should().Be(0);
            (await db.Discounts.SingleAsync()).UsedCount.Should().Be(0); (await db.CartItems.CountAsync()).Should().Be(1);
        });
    }
    [Fact]
    public async Task CommitAndRetry_UsesDurableReceipt_WithoutIncrementingAnyLedgerTwice()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync(Register);
        var buyer = await host.AddUser(); host.Authenticate(buyer); var request = await Seed(host, buyer);
        var first = await host.Send(new CheckoutCommand(request, "receipt")); first.IsSuccess.Should().BeTrue();
        var second = await host.Send(new CheckoutCommand(request, "receipt")); second.IsSuccess.Should().BeTrue();
        second.Value!.ParentOrderId.Should().Be(first.Value!.ParentOrderId);
        await host.WithDb(async db =>
        {
            (await db.ParentOrders.CountAsync()).Should().Be(1); (await db.InventoryHistories.CountAsync()).Should().Be(1);
            (await db.Inventories.SingleAsync()).ReservedQty.Should().Be(1); (await db.CartItems.CountAsync()).Should().Be(0);
            (await db.Discounts.SingleAsync()).UsedCount.Should().Be(1); (await db.DiscountUsages.CountAsync()).Should().Be(1);
            (await db.OutboxMessages.CountAsync()).Should().Be(1);
        });
    }
    [PostgresFact]
    public async Task TwoBuyersCheckoutLastSku_ExactlyOneSucceeds()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync(Register);
        var buyer1 = await host.AddUser(); var buyer2 = await host.AddUser("other@example.com", phone: "0900000001");
        var request1 = await Seed(host, buyer1); Guid sku = default;
        await host.WithDb(async db => sku = (await db.Skus.SingleAsync()).Id);
        var request2 = await Seed(host, buyer2, sku);
        async Task<Result<CheckoutResponse>> Checkout(Guid buyer, CheckoutRequest request)
        {
            using var scope = host.Services.CreateScope(); var sp = scope.ServiceProvider;
            var user = new Mock<ICurrentUser>(); user.SetupGet(u => u.IsAuthenticated).Returns(true); user.SetupGet(u => u.UserId).Returns(buyer);
            var builder = new CheckoutDraftBuilder(sp.GetRequiredService<ICartRepository>(), sp.GetRequiredService<ISkuRepository>(),
                sp.GetRequiredService<ISpuRepository>(), sp.GetRequiredService<IShopRepository>(), sp.GetRequiredService<IInventoryRepository>(),
                sp.GetRequiredService<IUserAddressRepository>(), sp.GetRequiredService<IShopAddressRepository>(), sp.GetRequiredService<IDiscountRepository>(),
                sp.GetRequiredService<IShippingProvider>(), user.Object, host.Clock, sp.GetRequiredService<IFlashSaleRepository>());
            var handler = new CheckoutCommandHandler(builder, sp.GetRequiredService<ITransactionManager>(), sp.GetRequiredService<ICartRepository>(),
                sp.GetRequiredService<IInventoryRepository>(), sp.GetRequiredService<IParentOrderRepository>(), sp.GetRequiredService<ISubOrderRepository>(),
                sp.GetRequiredService<IOrderItemRepository>(), sp.GetRequiredService<IOrderStatusHistoryRepository>(), sp.GetRequiredService<IPaymentRepository>(),
                sp.GetRequiredService<IDiscountRepository>(), sp.GetRequiredService<IOutboxRepository>(), user.Object, host.Clock, sp.GetRequiredService<IFlashSaleRepository>());
            return await handler.Handle(new(request with { PlatformProductVoucherCode = null }, buyer.ToString()), default);
        }
        var results = await Task.WhenAll(Checkout(buyer1, request1), Checkout(buyer2, request2));
        results.Count(r => r.IsSuccess).Should().Be(1);
        await host.WithDb(async db =>
        {
            var stock = await db.Inventories.SingleAsync(); stock.ReservedQty.Should().Be(1);
            stock.QtyOnHand.Should().Be(1); (await db.ParentOrders.CountAsync()).Should().Be(1);
        });
    }
}
