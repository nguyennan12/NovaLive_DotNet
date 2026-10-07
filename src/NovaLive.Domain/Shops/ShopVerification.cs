using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class ShopVerification : Entity
{
    public Guid ShopId { get; set; }

    public Guid SubmittedBy { get; set; }

    public string IdCardFront { get; set; } = string.Empty;

    public string IdCardBack { get; set; } = string.Empty;

    public string? BusinessLicense { get; set; }

    public string BankAccount { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;

    public string? BankBranch { get; set; }

    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

    public Guid? ReviewerId { get; set; }

    public string? ReviewerNote { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
}
