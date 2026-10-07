using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NovaLive.Api.Hubs;

[Authorize]
public sealed class OrderNotificationHub : Hub
{
    public Task JoinShop(Guid shopId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"shop_orders_{shopId:N}");
    }

    public Task LeaveShop(Guid shopId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"shop_orders_{shopId:N}");
    }

    public Task JoinCustomer(Guid customerId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"customer_orders_{customerId:N}");
    }
}
