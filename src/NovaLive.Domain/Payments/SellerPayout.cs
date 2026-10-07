using NovaLive.Domain.Common;

namespace NovaLive.Domain.Payments;

public sealed class SellerPayout : AuditableEntity
{
    public Guid ShopId { get; private set; }

    public decimal Amount { get; private set; }

    public string BankAccount { get; private set; } = string.Empty;

    public string BankName { get; private set; } = string.Empty;

    public string? TransferRef { get; private set; }

    public PayoutStatus Status { get; private set; } = PayoutStatus.Pending;

    public Guid? ProcessedBy { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset ScheduledAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }
}
