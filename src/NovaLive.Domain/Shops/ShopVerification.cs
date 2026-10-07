using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class ShopVerification : Entity
{
    public Guid ShopId { get; private set; }

    public Guid SubmittedBy { get; private set; }

    public string IdCardFront { get; private set; } = string.Empty;

    public string IdCardBack { get; private set; } = string.Empty;

    public string? BusinessLicense { get; private set; }

    public string BankAccount { get; private set; } = string.Empty;

    public string BankName { get; private set; } = string.Empty;

    public string? BankBranch { get; private set; }

    public VerificationStatus Status { get; private set; } = VerificationStatus.Pending;

    public Guid? ReviewerId { get; private set; }

    public string? ReviewerNote { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public DateTimeOffset SubmittedAt { get; private set; } = DateTimeOffset.UtcNow;
}
