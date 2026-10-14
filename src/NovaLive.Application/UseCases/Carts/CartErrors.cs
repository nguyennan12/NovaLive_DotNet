using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Carts;
public static class CartErrors
{
    public static readonly Error Unauthorized = Error.Unauthorized("Carts.Unauthorized", "Bạn cần đăng nhập.");
    public static readonly Error ItemNotFound = Error.NotFound("Carts.ItemNotFound", "Không tìm thấy sản phẩm trong giỏ hàng.");
    public static readonly Error SkuNotFound = Error.NotFound("Carts.SkuNotFound", "Không tìm thấy SKU.");
    public static readonly Error SkuUnavailable = Error.Validation("Carts.SkuUnavailable", "SKU hiện không còn mở bán.");
    public static readonly Error SpuUnavailable = Error.Validation("Carts.SpuUnavailable", "Sản phẩm hiện không còn mở bán.");
    public static readonly Error ShopUnavailable = Error.Validation("Carts.ShopUnavailable", "Gian hàng hiện không hoạt động.");
    public static readonly Error OutOfStock = Error.Validation("Carts.OutOfStock", "Sản phẩm đã hết hàng.");
    public static readonly Error InsufficientStock = Error.Validation("Carts.InsufficientStock", "Số lượng vượt quá tồn kho khả dụng.");
    public static readonly Error ConcurrentChange = Error.Conflict("Carts.ConcurrentChange", "Giỏ hàng đang được cập nhật. Vui lòng thử lại.");
}
