using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Wallets;

public static class WalletErrors
{
    public static readonly Error NotFound = Error.NotFound("Wallets.NotFound", "Không tìm thấy ví của gian hàng.");
    public static readonly Error InsufficientBalance = WalletStateErrors.InsufficientPayoutBalance;
    public static readonly Error MinimumAmount = WalletStateErrors.MinimumPayoutAmount;
    public static readonly Error KycRequired = Error.Validation("Wallets.KycRequired", "Gian hàng cần hoàn tất xác thực KYC và có thông tin tài khoản ngân hàng trước khi yêu cầu rút tiền.");
    public static readonly Error ShopNotActive = Error.Forbidden("Wallets.ShopNotActive", "Gian hàng hiện không ở trạng thái hoạt động để thực hiện giao dịch ví.");
    public static readonly Error PayoutNotFound = Error.NotFound("Wallets.PayoutNotFound", "Không tìm thấy yêu cầu rút tiền.");
    public static readonly Error PayoutNotPending = Error.Validation("Wallets.PayoutNotPending", "Yêu cầu rút tiền không ở trạng thái chờ xử lý.");
    public static readonly Error Unauthorized = Error.Unauthorized("Wallets.Unauthorized", "Vui lòng đăng nhập.");
    public static readonly Error ShopNotFound = Error.NotFound("Wallets.ShopNotFound", "Không tìm thấy thông tin gian hàng.");
}
