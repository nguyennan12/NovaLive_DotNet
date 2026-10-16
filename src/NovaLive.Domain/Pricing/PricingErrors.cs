using NovaLive.Domain.Common;

namespace NovaLive.Domain.Pricing;

public static class PricingErrors
{
    public static readonly Error VoucherUsesExhausted = Error.Validation(
        "Voucher.UsesExhausted", "Voucher đã hết lượt sử dụng trên hệ thống.");
    public static readonly Error VoucherUserLimitReached = Error.Validation(
        "Voucher.UserLimitReached", "Bạn đã sử dụng hết lượt cho phép của voucher.");
    public static Error InvalidVoucher(string code, string? reason) => Error.Validation(code, reason);

    // Input validation errors (returns Result.Failure)
    public static readonly Error EmptyShops = Error.Validation(
        "Pricing.EmptyShops", "Danh sách shop tính toán không được để trống.");

    public static readonly Error DuplicateShop = Error.Validation(
        "Pricing.DuplicateShop", "Tồn tại shop bị trùng lặp trong đơn hàng.");

    public static readonly Error InvalidQuantity = Error.Validation(
        "Pricing.InvalidQuantity", "Số lượng sản phẩm phải lớn hơn 0.");

    public static readonly Error NegativePrice = Error.Validation(
        "Pricing.NegativePrice", "Đơn giá sản phẩm không được âm.");

    public static readonly Error NegativeShippingFee = Error.Validation(
        "Pricing.NegativeShippingFee", "Phí vận chuyển không được âm.");

    public static readonly Error NonIntegerAmount = Error.Validation(
        "Pricing.NonIntegerAmount", "Giá tiền hoặc phí vận chuyển tại thị trường VND phải là số nguyên.");

    // Slot configuration errors (returns Result.Failure)
    public static readonly Error ShopMismatch = Error.Validation(
        "Pricing.ShopMismatch", "Voucher shop không thuộc về shop tương ứng.");

    public static readonly Error PlatformScopeMismatch = Error.Validation(
        "Pricing.PlatformScopeMismatch", "Voucher sàn không được liên kết với shop cụ thể nào.");

    public static readonly Error InvalidVoucherTypeForSlot = Error.Validation(
        "Pricing.InvalidVoucherTypeForSlot", "Loại voucher không hợp lệ với slot áp dụng.");

    // Invariant failure error
    public static readonly Error InternalAccountingMismatch = Error.Failure(
        "Pricing.InternalAccountingMismatch", "Phát hiện sai số kế toán nội bộ trong quá trình tính toán giá.");

    // Voucher evaluation error codes (used in VoucherEvaluationResult)
    public const string VoucherExpired = "Voucher.Expired";
    public const string VoucherNotYetActive = "Voucher.NotYetActive";
    public const string VoucherInactive = "Voucher.Inactive";
    public const string MinOrderAmountNotMet = "Voucher.MinOrderAmountNotMet";
    public const string NoEligibleItems = "Voucher.NoEligibleItems";
}
