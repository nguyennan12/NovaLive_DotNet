using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Livestreams;

public static class LivestreamErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Livestream.NotFound", "Không tìm thấy phiên livestream.");

    public static readonly Error ShopAlreadyHasActiveLive =
        Error.Conflict("Livestream.ShopAlreadyHasActiveLive", "Gian hàng hiện đã có một phiên livestream đang phát trực tiếp.");

    public static readonly Error UnauthorizedShop =
        Error.Unauthorized("Livestream.UnauthorizedShop", "Không tìm thấy thông tin gian hàng của người dùng.");

    public static readonly Error ForbiddenShop =
        Error.Forbidden("Livestream.ForbiddenShop", "Bạn không có quyền quản lý phiên livestream của gian hàng khác.");

    public static readonly Error NotLive =
        Error.Conflict("Livestream.NotLive", "Phiên livestream hiện không ở trạng thái phát trực tiếp.");

    public static readonly Error AlreadyEnded =
        Error.Conflict("Livestream.AlreadyEnded", "Phiên livestream đã kết thúc.");

    public static readonly Error SkuNotFound =
        Error.NotFound("Livestream.SkuNotFound", "Không tìm thấy SKU sản phẩm.");

    public static readonly Error SkuNotBelongToShop =
        Error.Forbidden("Livestream.SkuNotBelongToShop", "SKU sản phẩm không thuộc quyền sở hữu của gian hàng.");

    public static readonly Error SkuInactive =
        Error.Validation("Livestream.SkuInactive", "SKU sản phẩm đang bị tắt hoặc không hoạt động.");

    public static readonly Error InvalidFlashPrice =
        Error.Validation("Livestream.InvalidFlashPrice", "Giá Flash phải lớn hơn 0 và không được vượt quá giá bán hiện tại của SKU.");

    public static readonly Error QuantityLimitExceedsStock =
        Error.Validation("Livestream.QuantityLimitExceedsStock", "Số lượng Flash Sale giới hạn không được vượt quá tồn kho khả dụng.");

    public static readonly Error InvalidDuration =
        Error.Validation("Livestream.InvalidDuration", "Thời hạn đếm ngược ghim sản phẩm phải từ 60 đến 300 giây.");

    public static readonly Error ProductNotPinned =
        Error.NotFound("Livestream.ProductNotPinned", "Sản phẩm hiện không ở trạng thái được ghim trong phiên live.");

    public static readonly Error WarehouseAddressNotFound =
        Error.NotFound("Livestream.WarehouseAddressNotFound", "Không tìm thấy kho xuất hàng của gian hàng.");
}
