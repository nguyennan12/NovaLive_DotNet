using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Discounts;

public static class DiscountsErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Discounts.NotFound", "Không tìm thấy mã giảm giá.");

    public static readonly Error CodeAlreadyExists =
        Error.Conflict("Discounts.CodeAlreadyExists", "Mã voucher đã tồn tại trên hệ thống.");

    public static readonly Error UnauthorizedShop =
        Error.Unauthorized("Discounts.UnauthorizedShop", "Không tìm thấy thông tin gian hàng của người dùng.");

    public static readonly Error ForbiddenShop =
        Error.Forbidden("Discounts.ForbiddenShop", "Bạn không có quyền quản lý voucher của gian hàng khác.");

    public static readonly Error CannotModifyUsedVoucher =
        Error.Validation("Discounts.CannotModifyUsedVoucher", "Không thể chỉnh sửa mã voucher hoặc giá trị giảm khi đã có người sử dụng.");

    public static readonly Error UsageLimitReached =
        Error.Validation("Discounts.UsageLimitReached", "Mã voucher đã hết lượt sử dụng trên hệ thống.");

    public static readonly Error UserLimitReached =
        Error.Validation("Discounts.UserLimitReached", "Bạn đã dùng hết số lượt cho phép của mã voucher này.");

    public static readonly Error Expired =
        Error.Validation("Discounts.Expired", "Mã voucher đã hết hạn sử dụng.");

    public static readonly Error NotYetActive =
        Error.Validation("Discounts.NotYetActive", "Mã voucher chưa đến thời gian áp dụng.");

    public static readonly Error Inactive =
        Error.Validation("Discounts.Inactive", "Mã voucher đang bị tắt kích hoạt.");

    public static readonly Error InvalidDateRange =
        Error.Validation("Discounts.InvalidDateRange", "Thời gian kết thúc (valid_to) phải lớn hơn thời gian bắt đầu (valid_from).");

    public static readonly Error InvalidDiscountValue =
        Error.Validation("Discounts.InvalidDiscountValue", "Giá trị giảm giá phải lớn hơn 0.");

    public static readonly Error InvalidPercentValue =
        Error.Validation("Discounts.InvalidPercentValue", "Phần trăm giảm giá phải trong khoảng từ 1% đến 100%.");

    public static readonly Error MaxDiscountAmountRequired =
        Error.Validation("Discounts.MaxDiscountAmountRequired", "Giảm giá phần trăm bắt buộc phải có hạn mức giảm tối đa lớn hơn 0.");

    public static readonly Error InvalidType =
        Error.Validation("Discounts.InvalidType", "Loại voucher không hợp lệ với phân quyền này.");

    public static readonly Error EmptyCartItems =
        Error.Validation("Discounts.EmptyCartItems", "Danh sách sản phẩm giỏ hàng để áp dụng voucher không được để trống.");

    public static readonly Error MinOrderAmountNotMet =
        Error.Validation("Discounts.MinOrderAmountNotMet", "Tổng giá trị đơn hàng chưa đạt mức tối thiểu áp dụng voucher.");

    public static readonly Error NoEligibleItems =
        Error.Validation("Discounts.NoEligibleItems", "Không có sản phẩm nào trong giỏ hàng thỏa mãn điều kiện áp dụng voucher.");

    public static readonly Error InvalidMaxUses =
        Error.Validation("Discounts.InvalidMaxUses", "Tổng số lượt dùng mới không được nhỏ hơn số lượt đã sử dụng.");

    public static readonly Error CannotDeleteUsedVoucher =
        Error.Validation("Discounts.CannotDeleteUsedVoucher", "Không thể xóa voucher đã có lượt sử dụng. Vui lòng tắt kích hoạt thay vì xóa.");
}
