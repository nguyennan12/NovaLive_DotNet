using MassTransit;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Domain.Notifications;

namespace NovaLive.Infrastructure.System;

public sealed class OrderPlacedConsumer(
    IUserRepository users,
    IShopRepository shops,
    INotificationRepository notifications,
    IOrderConfirmationEmailSender emailSender,
    IOrderNotificationService orderNotifications,
    IIdempotencyService idempotency,
    ILogger<OrderPlacedConsumer> logger) : IConsumer<OrderPlacedEvent>
{
    private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromDays(30);

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var order = context.Message;
        var ct = context.CancellationToken;
        var idempotencyKey = $"order-placed:{order.IdempotencyKey}";
        if (!await idempotency.TryAcquireAsync(idempotencyKey, TimeSpan.FromMinutes(10), ct))
        {
            logger.LogInformation("Order placed event already handled for parent order {ParentOrderId}.", order.ParentOrderId);
            return;
        }

        try
        {
            var buyer = await users.GetByIdAsync(order.BuyerId, ct)
                ?? throw new InvalidOperationException("Order placed event references a missing buyer.");
            var shopIds = order.SubOrders.Select(subOrder => subOrder.ShopId).Distinct().ToArray();
            var shopRecords = await shops.GetByIdsAsync(shopIds, ct);
            var shopsById = shopRecords.ToDictionary(shop => shop.Id);

            var pendingNotifications = new List<Notification>();
            if (!await notifications.ExistsForReferenceAsync(order.BuyerId, "ParentOrder", order.ParentOrderId, ct))
            {
                pendingNotifications.Add(new Notification(order.BuyerId, "OrderPlaced", "Đặt hàng thành công",
                    $"Đơn hàng {order.OrderCode} đã được tiếp nhận.", "ParentOrder", order.ParentOrderId));
            }

            foreach (var subOrder in order.SubOrders)
            {
                if (!shopsById.TryGetValue(subOrder.ShopId, out var shop))
                    throw new InvalidOperationException($"Order placed event references missing shop {subOrder.ShopId}.");

                if (!await notifications.ExistsForReferenceAsync(shop.OwnerId, "SubOrder", subOrder.SubOrderId, ct))
                {
                    pendingNotifications.Add(new Notification(shop.OwnerId, "OrderPlaced", "Có đơn hàng mới",
                        $"Gian hàng {shop.ShopName} có đơn mới {subOrder.SubOrderId}.", "SubOrder", subOrder.SubOrderId));
                }
            }

            await emailSender.SendAsync(buyer.Email, OrderPlacedEmailTemplate.Render(order), ct);
            await notifications.AddRangeAsync(pendingNotifications, ct);
            await notifications.SaveChangesAsync(ct);

            foreach (var subOrder in order.SubOrders)
            {
                await orderNotifications.NotifySellerAsync(new OrderPlacedSellerNotification(
                    subOrder.SubOrderId, subOrder.ShopId, subOrder.ItemCount, subOrder.TotalAmount), ct);
            }

            await idempotency.CreateAsync(idempotencyKey, nameof(OrderPlacedEvent), IdempotencyTtl, ct);
        }
        catch
        {
            await idempotency.RemoveAsync(idempotencyKey, ct);
            throw;
        }
    }
}
