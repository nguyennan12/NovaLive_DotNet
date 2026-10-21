using NovaLive.Domain.Common;

namespace NovaLive.Domain.Payments;

public sealed class SellerPayout : AuditableEntity
{
    private SellerPayout() { }

    public SellerPayout(
        Guid shopId,
        decimal amount,
        string bankAccount,
        string bankName,
        string? note = null,
        DateTimeOffset? scheduledAt = null)
    {
        ShopId = shopId;
        Amount = amount;
        BankAccount = bankAccount;
        BankName = bankName;
        Note = note;
        Status = PayoutStatus.Pending;
        ScheduledAt = scheduledAt ?? DateTimeOffset.UtcNow;
    }

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

    public static SellerPayout Create(
        Guid shopId,
        decimal amount,
        string bankAccount,
        string bankName,
        string? note = null,
        DateTimeOffset? scheduledAt = null) =>
        new(shopId, amount, bankAccount, bankName, note, scheduledAt);

    public void Approve(string transferRef, Guid processedBy, DateTimeOffset now)
    {
        Status = PayoutStatus.Completed;
        TransferRef = transferRef;
        ProcessedBy = processedBy;
        CompletedAt = now;
        UpdatedAt = now;
    }

    public void Reject(string reason, Guid processedBy, DateTimeOffset now)
    {
        Status = PayoutStatus.Failed;
        Note = reason;
        ProcessedBy = processedBy;
        CompletedAt = now;
        UpdatedAt = now;
    }
}
