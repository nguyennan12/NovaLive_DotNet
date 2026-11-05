using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Payments.Queries.GetPaymentStatus;

public sealed class GetPaymentStatusQueryHandler(ICurrentUser user, IPaymentRepository payments,
    IParentOrderRepository orders) : IQueryHandler<GetPaymentStatusQuery, PaymentStatusResponse>
{
    public async Task<Result<PaymentStatusResponse>> Handle(GetPaymentStatusQuery query, CancellationToken ct)
    {
        if (user.UserId is not { } buyer || buyer == Guid.Empty) return PaymentErrors.Unauthorized;
        var payment = await payments.GetByIdAsync(query.PaymentId, ct);
        if (payment is null) return PaymentErrors.PaymentNotFound;
        if (await orders.FindAsync(payment.ParentOrderId, buyer, ct) is null) return PaymentErrors.PaymentNotFound;
        // Read the authoritative database state on every poll, avoiding stale Pending cache entries.
        return new PaymentStatusResponse(payment.Id, payment.ParentOrderId, payment.Method.ToString(),
            payment.Amount, payment.Status.ToString(), payment.PaidAt, payment.ExpiredAt);
    }
}
