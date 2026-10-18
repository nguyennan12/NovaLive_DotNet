using Microsoft.AspNetCore.SignalR;
using NovaLive.Api.Hubs;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Api.Services;

public sealed class PaymentNotificationService(IHubContext<PaymentNotificationHub> hubContext) : IPaymentNotificationService
{
    public Task NotifyPaymentSuccessAsync(PaymentSuccessEvent payment, CancellationToken ct = default) =>
        hubContext.Clients.Group($"payment_{payment.PaymentId:N}")
            .SendAsync("PaymentSuccess", payment, ct);
}
