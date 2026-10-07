using NovaLive.Domain.Common;

namespace NovaLive.Domain.Payments;

public sealed class SellerPayout : AuditableEntity
{
    public Guid ShopId { get; set; }

    public decimal Amount { get; set; }

    public string BankAccount { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;

    public string? TransferRef { get; set; }

    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

    public Guid? ProcessedBy { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset ScheduledAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}
