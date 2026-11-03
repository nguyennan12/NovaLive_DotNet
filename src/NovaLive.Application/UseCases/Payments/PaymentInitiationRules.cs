using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;

namespace NovaLive.Application.UseCases.Payments;

public static class PaymentInitiationRules
{
    public static Result Validate(ParentOrder order, Payment payment, PaymentMethod method, DateTimeOffset now)
    {
        if (order.PaymentStatus != ParentOrderPaymentStatus.Pending || payment.Status != PaymentStatus.Pending)
            return Error.Conflict("Payment.NotPending", "Đơn hàng hoặc giao dịch không còn chờ thanh toán.");
        // The method chosen at checkout also controls fulfilment (in particular COD).
        if (payment.Method != method) return PaymentErrors.InvalidMethod;
        if (payment.IsDueForExpiry(now, TimeSpan.FromMinutes(15))) return PaymentErrors.PaymentExpired;
        if (payment.Amount <= 0 || payment.Amount != order.GrandTotal) return PaymentErrors.InvalidAmount;
        return Result.Success();
    }
}
