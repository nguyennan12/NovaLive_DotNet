using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class OrderReturn : AuditableEntity
{
    public Guid SubOrderId { get; private set; }

    public Guid BuyerId { get; private set; }

    public ReturnReason Reason { get; private set; }

    public List<string> EvidenceUrls { get; private set; } = [];

    public ReturnStatus Status { get; private set; } = ReturnStatus.Pending;

    public decimal RefundAmount { get; private set; }

    public string? SellerResponse { get; private set; }

    public string? AdminNote { get; private set; }

    public Guid? ResolvedBy { get; private set; }

    public DateTimeOffset? ResolvedAt { get; private set; }
}
