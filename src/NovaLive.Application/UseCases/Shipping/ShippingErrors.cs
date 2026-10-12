using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shipping;

public static class ShippingErrors
{
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
