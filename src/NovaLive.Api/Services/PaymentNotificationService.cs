using Microsoft.AspNetCore.SignalR;
using NovaLive.Api.Hubs;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Api.Services;

public sealed class PaymentNotificationService(IHubContext<PaymentNotificationHub> hubContext,
    IParentOrderRepository orders) : IPaymentNotificationService
{
    public async Task NotifyPaymentSuccessAsync(PaymentSuccessEvent payment, CancellationToken ct = default)
    {
        var order = await orders.FindAsync(payment.ParentOrderId, null, ct)
            ?? throw new InvalidOperationException($"Missing payment notification order {payment.ParentOrderId}.");
        await hubContext.Clients.User(order.BuyerId.ToString()).SendAsync("PaymentSuccess",
            new PaymentSuccessNotification(payment.PaymentId, order.Id, payment.Amount, payment.PaymentMethod), ct);
    }
}
