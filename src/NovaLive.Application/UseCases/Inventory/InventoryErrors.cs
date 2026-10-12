using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Inventory;

public static class InventoryErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Inventory.NotFound",
        "Không tìm thấy thông tin tồn kho của sản phẩm.");

    public static readonly Error SkuNotFound = Error.NotFound(
        "Inventory.SkuNotFound",
        "Không tìm thấy biến thể sản phẩm (SKU).");

    public static readonly Error UnauthorizedShop = Error.Unauthorized(
        "Inventory.UnauthorizedShop",
        "Bạn không có quyền thao tác trên kho hàng của gian hàng này.");

    public static readonly Error InsufficientStock = Error.Validation(
        "Inventory.InsufficientStock",
        "Số lượng tồn kho vật lý không thể nhỏ hơn số lượng đang giữ chỗ đặt hàng.");

    public static readonly Error InvalidQtyChange = Error.Validation(
        "Inventory.InvalidQtyChange",
        "Số lượng thay đổi tồn kho phải khác 0.");

    public static readonly Error InvalidChangeType = Error.Validation(
        "Inventory.InvalidChangeType",
        "Loại điều chỉnh tồn kho không hợp lệ. Chỉ chấp nhận Import hoặc ManualAdjust.");

    public static readonly Error CannotCancelMoreThanSold = Error.Validation(
        "Inventory.CannotCancelMoreThanSold",
        "Số lượng hủy xuất bán vượt quá số lượng đã bán thực tế của đơn hàng.");

    public static readonly Error CannotReturnMoreThanSold = Error.Validation(
        "Inventory.CannotReturnMoreThanSold",
        "Số lượng hoàn trả vượt quá số lượng đã bán thực tế của đơn hàng.");

    public static readonly Error MissingOrderReference = Error.Validation(
        "Inventory.MissingOrderReference",
        "Hủy bán hoặc trả hàng bắt buộc phải có thông tin mã đơn hàng tham chiếu.");
}
