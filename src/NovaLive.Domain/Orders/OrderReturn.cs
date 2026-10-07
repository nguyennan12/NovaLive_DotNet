using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class OrderReturn : AuditableEntity
{
    public Guid SubOrderId { get; set; }

    public Guid BuyerId { get; set; }

    public ReturnReason Reason { get; set; }

    public List<string> EvidenceUrls { get; set; } = [];

    public ReturnStatus Status { get; set; } = ReturnStatus.Pending;

    public decimal RefundAmount { get; set; }

    public string? SellerResponse { get; set; }

    public string? AdminNote { get; set; }

    public Guid? ResolvedBy { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }
}
