using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NovaLive.Application.UseCases.Payments.Queries.GetPaymentStatus;

namespace NovaLive.Api.Hubs;

[Authorize]
public sealed class PaymentNotificationHub(ISender sender) : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (!Guid.TryParse(Context.UserIdentifier, out var buyer) || buyer == Guid.Empty)
        {
            Context.Abort();
            throw new HubException("Không xác định được người mua.");
        }
        // SignalR maps ConnectionId to UserIdentifier and automatically removes it on disconnect.
        await base.OnConnectedAsync();
    }

    public async Task JoinPayment(Guid paymentId)
    {
        var result = await sender.Send(new GetPaymentStatusQuery(paymentId), Context.ConnectionAborted);
        if (result.IsFailure) throw new HubException("Không tìm thấy giao dịch thanh toán của bạn.");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"payment_{paymentId:N}", Context.ConnectionAborted);
    }

    public Task LeavePayment(Guid paymentId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"payment_{paymentId:N}", Context.ConnectionAborted);
}
