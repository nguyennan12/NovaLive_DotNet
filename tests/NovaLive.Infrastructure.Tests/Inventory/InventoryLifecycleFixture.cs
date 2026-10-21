using Microsoft.Extensions.DependencyInjection;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Products;
using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Infrastructure.Services;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.Inventory;

internal static class InventoryLifecycleFixture
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IParentOrderRepository, ParentOrderRepository>();
        services.AddScoped<ISubOrderRepository, SubOrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IInventoryHistoryRepository, InventoryHistoryRepository>();
        services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IFlashSaleRepository, FlashSaleRepository>();
        services.AddSingleton<IIdempotencyService, RedisIdempotencyService>();
    }

    public static async Task<Payment> SeedAsync(AuthTestHost host, bool paid = true,
        PaymentMethod method = PaymentMethod.MoMo, bool mismatch = false)
    {
        var now = host.Clock.UtcNow.AddMinutes(-20);
        var parent = ParentOrder.Create(Guid.NewGuid(), "NOVA-" + Guid.NewGuid().ToString("N"), "{}", 400, 0, 0, 400, null, now);
        var payment = Payment.Create(parent.Id, method, 400, now);
        if (paid) payment.MarkSuccess(Guid.NewGuid().ToString(), now);
        await host.WithDb(async db =>
        {
            db.ParentOrders.Add(parent); db.Payments.Add(payment);
            for (var index = 0; index < 2; index++)
            {
                var sku = new Sku(Guid.NewGuid(), Guid.NewGuid(), "SKU-" + index, "{}", 100, 100);
                var sub = SubOrder.Create(parent.Id, parent.OrderCode + "-S" + index, Guid.NewGuid(),
                    new ShopCalculationResult(sku.ShopId, 200, 0, 0, 0, 0, 200, 200, []), method == PaymentMethod.COD, now);
                var stock = new Domain.Inventory.Inventory(sku.Id, sku.ShopId, 10);
                stock.Reserve(mismatch ? 1 : 2);
                var line = OrderItem.Create(sub.Id, "{}", 100,
                    new OrderItemCalculationResult(sku.Id, sku.SpuId, 2, 100, 200, 0, 0, 0, 200));
                db.Skus.Add(sku); db.SubOrders.Add(sub); db.Inventories.Add(stock); db.OrderItems.Add(line);
            }
            await db.SaveChangesAsync();
        });
        return payment;
    }
}
