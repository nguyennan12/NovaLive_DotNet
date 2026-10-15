using NovaLive.Domain.Common;

namespace NovaLive.Domain.Payments;

/// <summary>
/// Thực thể thanh toán cho đơn hàng tổng (ParentOrder).
/// </summary>
public sealed class Payment : AuditableEntity
{
    private Payment() { }

    /// <summary>
    /// Khởi tạo bản ghi thanh toán cho đơn hàng.
    /// </summary>
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

    /// <summary>
    /// Chuyển trạng thái thanh toán sang Hết hạn (Expired) nếu đang ở trạng thái Chờ (Pending).
    /// </summary>
    public void ExpirePending(DateTimeOffset now)
    {
        if (Status != PaymentStatus.Pending) return;
        Status = PaymentStatus.Expired;
        ExpiredAt = now;
        UpdatedAt = now;
    }

    /// <summary>
    /// Đánh dấu thanh toán thành công khi nhận IPN/Webhook từ cổng thanh toán.
    /// </summary>
    /// <param name="transactionRef">Mã giao dịch từ cổng thanh toán (MoMo transId, mã giao dịch ngân hàng).</param>
    /// <param name="paidAt">Thời điểm thanh toán thành công.</param>
    /// <param name="gatewayResponse">Payload phản hồi lưu log từ cổng thanh toán.</param>
    public void MarkSuccess(string transactionRef, DateTimeOffset paidAt, string? gatewayResponse = null)
    {
        Status = PaymentStatus.Success;
        TransactionRef = transactionRef;
        PaidAt = paidAt;
        if (!string.IsNullOrWhiteSpace(gatewayResponse))
        {
            GatewayResponse = gatewayResponse;
        }
        UpdatedAt = paidAt;
    }

    /// <summary>
    /// Đánh dấu giao dịch thanh toán thất bại.
    /// </summary>
    public void MarkFailed(string? gatewayResponse, DateTimeOffset now)
    {
        if (Status == PaymentStatus.Success) return;
        Status = PaymentStatus.Failed;
        if (!string.IsNullOrWhiteSpace(gatewayResponse))
        {
            GatewayResponse = gatewayResponse;
        }
        UpdatedAt = now;
    }
}
