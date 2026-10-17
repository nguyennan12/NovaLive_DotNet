using NovaLive.Domain.Common;
namespace NovaLive.Domain.Orders;
public sealed class OrderReturn : AuditableEntity
{
    private OrderReturn() { }
    public static OrderReturn Create(Guid subOrderId, Guid buyerId, ReturnReason reason,
        IEnumerable<string> evidence, decimal refund, DateTimeOffset now) => new()
    {
        SubOrderId = subOrderId, BuyerId = buyerId, Reason = reason, EvidenceUrls = evidence.ToList(),
        RefundAmount = refund, Status = ReturnStatus.Pending, CreatedAt = now, UpdatedAt = now
    };
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

    public Result Approve(string response, DateTimeOffset now)
    {
        var guard = CanRespond(now);
        if (guard.IsFailure) return guard;
        Status = ReturnStatus.SellerApproved; SellerResponse = response; UpdatedAt = now;
        return Result.Success();
    }

    public Result Reject(string response, DateTimeOffset now)
    {
        var guard = CanRespond(now);
        if (guard.IsFailure) return guard;
        if (string.IsNullOrWhiteSpace(response)) return ReturnStateErrors.RejectionRequired;
        Status = ReturnStatus.SellerRejected; SellerResponse = response; UpdatedAt = now;
        return Result.Success();
    }

    private Result CanRespond(DateTimeOffset now)
    {
        if (Status != ReturnStatus.Pending) return ReturnStateErrors.InvalidStatus;
        if (now > CreatedAt.AddDays(3)) return ReturnStateErrors.ResponseExpired;
        return Result.Success();
    }

    public Result ConfirmReceived(Guid seller, DateTimeOffset now)
    {
        if (Status != ReturnStatus.SellerApproved) return ReturnStateErrors.InvalidStatus;
        Status = ReturnStatus.Completed; ResolvedBy = seller; ResolvedAt = now; UpdatedAt = now;
        return Result.Success();
    }
}
