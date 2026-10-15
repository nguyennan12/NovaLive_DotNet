using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Payments;

/// <summary>
/// Định nghĩa các mã lỗi liên quan đến quy trình thanh toán và webhook.
/// </summary>
public static class PaymentErrors
{
    /// <summary>Lỗi chữ ký bảo mật không khớp hoặc không hợp lệ.</summary>
    public static readonly Error InvalidSignature =
        Error.Validation("Payment.InvalidSignature", "Chữ ký bảo mật HMAC-SHA256 không hợp lệ.");

    /// <summary>Lỗi không tìm thấy bản ghi thanh toán.</summary>
    public static readonly Error PaymentNotFound =
        Error.NotFound("Payment.NotFound", "Không tìm thấy thông tin giao dịch thanh toán.");

    /// <summary>Lỗi không tìm thấy đơn hàng tương ứng.</summary>
    public static readonly Error OrderNotFound =
        Error.NotFound("Payment.OrderNotFound", "Không tìm thấy thông tin đơn hàng thanh toán.");

    /// <summary>Lỗi số tiền thanh toán không khớp với số tiền của đơn hàng.</summary>
    public static readonly Error InvalidAmount =
        Error.Validation("Payment.InvalidAmount", "Số tiền thanh toán thực tế không khớp với giá trị đơn hàng.");

    /// <summary>Lỗi phương thức thanh toán không được hỗ trợ.</summary>
    public static readonly Error InvalidMethod =
        Error.Validation("Payment.InvalidMethod", "Phương thức thanh toán không hợp lệ hoặc chưa được hỗ trợ.");

    /// <summary>Lỗi giao dịch thanh toán đã hết hạn.</summary>
    public static readonly Error PaymentExpired =
        Error.Validation("Payment.Expired", "Giao dịch thanh toán đã hết hạn.");

    /// <summary>Lỗi không thể bóc tách orderCode từ nội dung chuyển khoản.</summary>
    public static readonly Error CannotParseOrderCode =
        Error.Validation("Payment.CannotParseOrderCode", "Không thể trích xuất mã đơn hàng từ nội dung chuyển khoản.");

    /// <summary>Lỗi thiếu mã giao dịch định danh từ cổng thanh toán / ngân hàng.</summary>
    public static readonly Error MissingTransactionId =
        Error.Validation("Payment.MissingTransactionId", "Mã định danh giao dịch từ cổng thanh toán không được để trống.");

    /// <summary>Lỗi người dùng không có quyền thanh toán đơn hàng này.</summary>
    public static readonly Error Unauthorized =
        Error.Unauthorized("Payment.Unauthorized", "Bạn không có quyền thực hiện thao tác trên đơn hàng này.");

    /// <summary>Lỗi phát sinh từ phía cổng thanh toán bên thứ ba.</summary>
    public static Error GatewayFailed(string message) =>
        Error.Failure("Payment.GatewayFailed", message);
}
