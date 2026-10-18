using Microsoft.AspNetCore.SignalR;
using NovaLive.Api.Hubs;
using NovaLive.Application.Abstractions.Services;

namespace NovaLive.Api.Services;

public sealed class OrderNotificationService(IHubContext<OrderNotificationHub> hubContext) : IOrderNotificationService
{
    public Task NotifySellerAsync(OrderPlacedSellerNotification notification, CancellationToken ct = default) =>
        hubContext.Clients.Group($"shop_orders_{notification.ShopId:N}")
            .SendAsync("OrderPlaced", notification, ct);
}
