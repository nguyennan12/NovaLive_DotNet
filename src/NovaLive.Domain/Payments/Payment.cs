using NovaLive.Domain.Common;

namespace NovaLive.Domain.Payments;

public sealed class Payment : AuditableEntity
{
    public Guid ParentOrderId { get; private set; }

    public PaymentMethod Method { get; private set; }

    public decimal Amount { get; private set; }

    public string? TransactionRef { get; private set; }

    public string? GatewayResponse { get; private set; }

    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;

    public DateTimeOffset? PaidAt { get; private set; }

    public DateTimeOffset? ExpiredAt { get; private set; }
}
