using NovaLive.Domain.Common;

namespace NovaLive.Domain.Payments;

public sealed class PaymentEscrow : AuditableEntity
{
    public Guid SubOrderId { get; set; }

    public Guid PaymentId { get; set; }

    public Guid ShopId { get; set; }

    public decimal HeldAmount { get; set; }

    public decimal PlatformFee { get; set; }

    public EscrowStatus Status { get; set; } = EscrowStatus.PendingCapture;

    public DateTimeOffset? HoldUntil { get; set; }

    public DateTimeOffset? ReleasedAt { get; set; }

    public DateTimeOffset? RefundedAt { get; set; }
}
