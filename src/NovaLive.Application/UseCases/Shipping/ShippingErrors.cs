using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shipping;

public static class ShippingErrors
{
    public static readonly Error SubOrderNotFound =
        Error.NotFound("Shipping.SubOrderNotFound", "Không tìm thấy đơn hàng của gian hàng.");
    public static readonly Error ShipmentNotFound =
        Error.NotFound("Shipping.ShipmentNotFound", "Đơn hàng chưa có vận đơn.");
    public static readonly Error ShipmentAlreadyCreated =
        Error.Conflict("Shipping.ShipmentAlreadyCreated", "Đơn hàng đã có vận đơn.");
    public static readonly Error InvalidOrderStatus =
        Error.Conflict("Shipping.InvalidOrderStatus", "Chỉ có thể tạo vận đơn cho đơn hàng Confirmed.");
    public static readonly Error InvalidServiceCode =
        Error.Validation("Shipping.InvalidServiceCode", "Mã dịch vụ vận chuyển không hợp lệ.");
    public static readonly Error InvalidItems =
        Error.Validation("Shipping.InvalidItems", "Sản phẩm hoặc tổng khối lượng vận chuyển không hợp lệ.");
    public static readonly Error PaymentNotFound =
        Error.Conflict("Shipping.PaymentNotFound", "Không tìm thấy phương thức thanh toán của đơn hàng.");
    public static readonly Error Unauthorized =
        Error.Unauthorized("Shipping.Unauthorized", "Vui lòng đăng nhập.");
    public static readonly Error UnsupportedProvider =
        Error.Validation("Shipping.UnsupportedProvider", "Đơn vị vận chuyển không được hỗ trợ.");
    public static readonly Error ProviderNotAvailable =
        Error.Conflict("Shipping.ProviderNotAvailable", "Đơn vị vận chuyển chưa được tích hợp.");

    public static readonly Error WarehouseNotFound =
        Error.NotFound("Shipping.WarehouseNotFound", "Không tìm thấy thông tin kho xuất hàng.");

    public static readonly Error InvalidShopWarehouse =
        Error.Validation("Shipping.InvalidShopWarehouse", "Kho xuất hàng không thuộc về gian hàng đã chọn.");

    public static readonly Error IncompleteWarehouseAddress =
        Error.Validation("Shipping.IncompleteWarehouseAddress", "Địa chỉ kho hàng chưa đầy đủ thông tin Quận/Huyện hoặc Phường/Xã để tính phí.");

    public static readonly Error RecipientAddressNotFound =
        Error.NotFound("Shipping.RecipientAddressNotFound", "Không tìm thấy thông tin địa chỉ giao hàng.");

    public static readonly Error IncompleteRecipientAddress =
        Error.Validation("Shipping.IncompleteRecipientAddress", "Địa chỉ người nhận chưa đầy đủ thông tin Quận/Huyện hoặc Phường/Xã để tính phí.");

    public static readonly Error OrderNotFound =
        Error.NotFound("Shipping.OrderNotFound", "Không tìm thấy đơn vận chuyển tương ứng.");

    public static Error CalculationFailed(string? message = null) =>
        Error.Validation("Shipping.CalculationFailed", message ?? "Không thể tính cước vận chuyển từ đối tác.");

    public static Error ProviderError(string code, string? message) =>
        Error.Validation(code, message ?? "Lỗi từ đơn vị vận chuyển.");
}
