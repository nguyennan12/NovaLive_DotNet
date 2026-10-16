using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Orders;

public static class CheckoutErrors
{
    public static readonly Error Unauthorized = Error.Unauthorized("Checkout.Unauthorized", "Vui lòng đăng nhập để tính nháp thanh toán.");
    public static readonly Error CartItemsNotFound = Error.NotFound("Checkout.CartItemsNotFound", "Không tìm thấy đầy đủ sản phẩm được chọn trong giỏ hàng của bạn.");
    public static readonly Error AddressNotFound = Error.NotFound("Checkout.AddressNotFound", "Không tìm thấy địa chỉ nhận hàng của bạn.");
    public static readonly Error IncompleteAddress = Error.Validation("Checkout.IncompleteAddress", "Địa chỉ nhận hàng thiếu mã quận/huyện hoặc phường/xã hợp lệ.");
    public static Error SkuUnavailable(Guid id) => Error.Validation("Checkout.SkuUnavailable", $"SKU '{id}' không còn mở bán.");
    public static Error ProductUnavailable(Guid id) => Error.Validation("Checkout.ProductUnavailable", $"Sản phẩm '{id}' không còn mở bán.");
    public static Error ShopUnavailable(Guid id) => Error.Validation("Checkout.ShopUnavailable", $"Gian hàng '{id}' không còn hoạt động.");
    public static Error InsufficientStock(Guid id) => Error.Validation("Checkout.InsufficientStock", $"SKU '{id}' không đủ tồn kho khả dụng.");
    public static Error PrimaryWarehouseMissing(Guid id) => Error.Validation("Checkout.PrimaryWarehouseMissing", $"Gian hàng '{id}' chưa có kho lấy hàng mặc định duy nhất.");
    public static Error InvalidWarehouseAddress(Guid id) => Error.Validation("Checkout.InvalidWarehouseAddress", $"Kho của gian hàng '{id}' thiếu mã quận/huyện hoặc phường/xã hợp lệ.");
    public static Error InvalidWeight(Guid id) => Error.Validation("Checkout.InvalidWeight", $"Tổng khối lượng của gian hàng '{id}' không hợp lệ.");
    public static Error InvalidVoucher(string code, string reason) => Error.Validation("Checkout.InvalidVoucher", $"Voucher '{code}': {reason}");
    public static Error ShippingFailed(Guid shopId) => Error.Failure("Checkout.ShippingFailed", $"Không thể lấy cước vận chuyển cho gian hàng '{shopId}'. Vui lòng thử lại.");
}
