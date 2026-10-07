using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NovaLive.Api.Hubs;

[Authorize]
public sealed class PaymentNotificationHub : Hub
{
    public Task JoinPayment(Guid paymentId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"payment_{paymentId:N}");
    }

    public Task LeavePayment(Guid paymentId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"payment_{paymentId:N}");
    }
}
