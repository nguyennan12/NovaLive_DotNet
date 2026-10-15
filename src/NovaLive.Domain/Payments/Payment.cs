using NovaLive.Domain.Common;

namespace NovaLive.Domain.Payments;

public sealed class Payment : AuditableEntity
{
    private Payment() { }
    public static Payment Create(Guid parentId, PaymentMethod method, decimal amount, DateTimeOffset now) => new()
    {
        ParentOrderId = parentId, Method = method, Amount = amount,
        ExpiredAt = method == PaymentMethod.COD ? null : now.AddMinutes(15), CreatedAt = now, UpdatedAt = now
        // Direct settlement, no escrow. T30 initiates the gateway; T32 credits shop wallets.
    };

    public Guid ParentOrderId { get; private set; }

    public PaymentMethod Method { get; private set; }

    public decimal Amount { get; private set; }

    public string? TransactionRef { get; private set; }

    public string? GatewayResponse { get; private set; }

    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;

    public DateTimeOffset? PaidAt { get; private set; }

    public DateTimeOffset? ExpiredAt { get; private set; }
    public void ExpirePending(DateTimeOffset now)
    {
        if (Status != PaymentStatus.Pending) return;
        Status = PaymentStatus.Expired; ExpiredAt = now; UpdatedAt = now;
    }
}
