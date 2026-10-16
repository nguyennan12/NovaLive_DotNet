using NovaLive.Domain.Common;

namespace NovaLive.Domain.Payments;

public sealed class Payment : AuditableEntity
{
    private Payment() { }

    public static Payment Create(Guid parentId, PaymentMethod method, decimal amount, DateTimeOffset now) => new()
    {
        ParentOrderId = parentId,
        Method = method,
        Amount = amount,
        ExpiredAt = method == PaymentMethod.COD ? null : now.AddMinutes(15),
        CreatedAt = now,
        UpdatedAt = now
    };

    public Guid ParentOrderId { get; private set; }

    public PaymentMethod Method { get; private set; }

    public decimal Amount { get; private set; }

    public string? TransactionRef { get; private set; }

    public string? GatewayResponse { get; private set; }

    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;

    public DateTimeOffset? PaidAt { get; private set; }

    public DateTimeOffset? ExpiredAt { get; private set; }

    public void ExpirePending(DateTimeOffset now)
    {
        if (Status != PaymentStatus.Pending) return;
        Status = PaymentStatus.Expired;
        ExpiredAt = now;
        UpdatedAt = now;
    }

    public Result MarkSuccess(string transactionRef, DateTimeOffset paidAt, string? gatewayResponse = null)
    {
        if (Status == PaymentStatus.Success) return Result.Success();
        if (Status is PaymentStatus.Expired or PaymentStatus.Failed or PaymentStatus.Refunded)
        {
            return Result.Failure(Error.Conflict("Payment.TerminalState",
                $"Không thể chuyển giao dịch sang trạng thái thành công khi đang ở trạng thái {Status}."));
        }

        Status = PaymentStatus.Success;
        TransactionRef = transactionRef;
        PaidAt = paidAt;
        if (!string.IsNullOrWhiteSpace(gatewayResponse))
        {
            GatewayResponse = gatewayResponse;
        }
        UpdatedAt = paidAt;
        return Result.Success();
    }

    public Result MarkFailed(string? gatewayResponse, DateTimeOffset now)
    {
        if (Status == PaymentStatus.Success)
        {
            return Result.Failure(Error.Conflict("Payment.AlreadySuccess",
                "Không thể đánh dấu thất bại khi giao dịch đã thanh toán thành công."));
        }

        if (Status == PaymentStatus.Failed) return Result.Success();

        Status = PaymentStatus.Failed;
        if (!string.IsNullOrWhiteSpace(gatewayResponse))
        {
            GatewayResponse = gatewayResponse;
        }
        UpdatedAt = now;
        return Result.Success();
    }
}
