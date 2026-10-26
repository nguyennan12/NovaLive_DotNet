using NovaLive.Domain.Common;
namespace NovaLive.Domain.Shops;
public static class WalletStateErrors
{
    public static readonly Error InsufficientBalance = Error.Validation("Returns.InsufficientShopBalance", "Số dư khả dụng của Shop không đủ để hoàn tiền.");
    public static readonly Error InsufficientPayoutBalance = Error.Validation("Wallet.InsufficientBalance", "Số dư khả dụng không đủ để thực hiện yêu cầu rút tiền.");
    public static readonly Error MinimumPayoutAmount = Error.Validation("Wallet.MinimumPayoutAmount", "Số tiền rút tối thiểu là 50.000 VNĐ.");
    public static readonly Error InsufficientLockedBalance = Error.Validation("Wallet.InsufficientLockedBalance", "Số dư tạm giữ không đủ.");
}
