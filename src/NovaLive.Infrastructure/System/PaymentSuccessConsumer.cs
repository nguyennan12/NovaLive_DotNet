using MassTransit;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Infrastructure.System;

public sealed class PaymentSuccessConsumer(
    IPaymentNotificationService notifications,
    IIdempotencyService idempotency) : IConsumer<PaymentSuccessEvent>
{
    private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromDays(30);

    public Task Consume(ConsumeContext<PaymentSuccessEvent> context) =>
        NotifyOnceAsync(context);

    private async Task NotifyOnceAsync(ConsumeContext<PaymentSuccessEvent> context)
    {
        var key = $"payment-success:{context.Message.PaymentId:N}";
        if (!await idempotency.TryAcquireAsync(key, TimeSpan.FromMinutes(10), context.CancellationToken))
            return;

        try
        {
            await notifications.NotifyPaymentSuccessAsync(context.Message, context.CancellationToken);
            await idempotency.CreateAsync(key, nameof(PaymentSuccessEvent), IdempotencyTtl, context.CancellationToken);
        }
        catch
        {
            await idempotency.RemoveAsync(key, context.CancellationToken);
            throw;
        }
    }
}
