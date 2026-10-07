using NovaLive.Domain.Common;

namespace NovaLive.Domain.Payments;

public sealed class Payment : AuditableEntity
{
    public Guid ParentOrderId { get; set; }

    public PaymentMethod Method { get; set; }

    public decimal Amount { get; set; }

    public string? TransactionRef { get; set; }

    public string? GatewayResponse { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTimeOffset? PaidAt { get; set; }

    public DateTimeOffset? ExpiredAt { get; set; }
}
