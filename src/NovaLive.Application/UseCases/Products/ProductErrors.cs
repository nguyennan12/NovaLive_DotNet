using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Products;

public static class ProductErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Product.NotFound", "Không tìm thấy sản phẩm hoặc sản phẩm đã bị ẩn/xóa.");

    public static readonly Error SkuNotFound =
        Error.NotFound("Product.SkuNotFound", "Không tìm thấy biến thể SKU hoặc bạn không có quyền chỉnh sửa.");

    public static readonly Error CategoryNotFound =
        Error.NotFound("Category.NotFound", "Danh mục sản phẩm được chọn không tồn tại.");

    public static readonly Error UnauthorizedShop =
        Error.Unauthorized("Auth.UnauthorizedShop", "Không tìm thấy thông tin gian hàng của người dùng.");

    public static Error DuplicateSkuCodes(IEnumerable<string> codes) =>
        Error.Conflict("Product.DuplicateSkuCode", $"Mã SKU đã tồn tại trong gian hàng: {string.Join(", ", codes)}");

    public static Error DuplicateSkuCode(string code) =>
        Error.Conflict("Product.DuplicateSkuCode", $"Mã SKU '{code}' đã tồn tại trong gian hàng.");
}
