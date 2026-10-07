using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NovaLive.RealtimeApi.Hubs;

[Authorize]
public sealed class OrderNotificationHub : Hub
{
    public Task JoinShop(Guid shopId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"shop_orders_{shopId:N}");
    }
}
