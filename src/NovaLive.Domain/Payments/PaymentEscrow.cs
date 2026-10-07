using NovaLive.Domain.Common;

namespace NovaLive.Domain.Payments;

public sealed class PaymentEscrow : AuditableEntity
{
    public Guid SubOrderId { get; private set; }

    public Guid PaymentId { get; private set; }

    public Guid ShopId { get; private set; }

    public decimal HeldAmount { get; private set; }

    public decimal PlatformFee { get; private set; }

    public EscrowStatus Status { get; private set; } = EscrowStatus.PendingCapture;

    public DateTimeOffset? HoldUntil { get; private set; }

    public DateTimeOffset? ReleasedAt { get; private set; }

    public DateTimeOffset? RefundedAt { get; private set; }
}
