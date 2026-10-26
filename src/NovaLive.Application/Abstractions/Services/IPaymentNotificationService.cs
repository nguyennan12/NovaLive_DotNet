using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Application.Abstractions.Services;

public interface IPaymentNotificationService
{
    Task NotifyPaymentSuccessAsync(PaymentSuccessEvent payment, CancellationToken ct = default);
}
