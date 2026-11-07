using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Payments;

public static class PaymentErrors
{
    public static readonly Error InvalidSignature =
        Error.Validation("Payment.InvalidSignature", "Chữ ký bảo mật HMAC-SHA256 không hợp lệ.");

    public static readonly Error PaymentNotFound =
        Error.NotFound("Payment.NotFound", "Không tìm thấy thông tin giao dịch thanh toán.");

    public static readonly Error OrderNotFound =
        Error.NotFound("Payment.OrderNotFound", "Không tìm thấy thông tin đơn hàng thanh toán.");

    public static readonly Error InvalidAmount =
        Error.Validation("Payment.InvalidAmount", "Số tiền thanh toán thực tế không khớp với giá trị đơn hàng.");

    public static readonly Error InvalidTransactionType =
        Error.Validation("Payment.InvalidTransactionType", "VietQR chỉ chấp nhận giao dịch tiền vào (transType=C).");

    public static readonly Error InvalidReceivingAccount =
        Error.Validation("Payment.InvalidReceivingAccount", "Tài khoản nhận tiền trong callback VietQR không khớp cấu hình.");

    public static readonly Error InvalidProviderOrderId =
        Error.Validation("Payment.InvalidProviderOrderId", "Mã đơn hàng VietQR trong callback không khớp giao dịch đã tạo.");

    public static readonly Error DuplicateTransaction =
        Error.Conflict("Payment.DuplicateTransaction", "Mã giao dịch VietQR đã được sử dụng.");

    public static readonly Error InvalidMethod =
        Error.Validation("Payment.InvalidMethod", "Phương thức thanh toán không hợp lệ hoặc chưa được hỗ trợ.");

    public static readonly Error PaymentExpired =
        Error.Validation("Payment.Expired", "Giao dịch thanh toán đã hết hạn.");

    public static readonly Error CannotParseOrderCode =
        Error.Validation("Payment.CannotParseOrderCode", "Không thể trích xuất mã đơn hàng từ nội dung chuyển khoản.");

    public static readonly Error MissingTransactionId =
        Error.Validation("Payment.MissingTransactionId", "Mã định danh giao dịch từ cổng thanh toán không được để trống.");

    public static readonly Error Unauthorized =
        Error.Unauthorized("Payment.Unauthorized", "Bạn không có quyền thực hiện thao tác trên đơn hàng này.");

    public static Error GatewayFailed(string message) =>
        Error.Failure("Payment.GatewayFailed", message);
}
