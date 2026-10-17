using NovaLive.Domain.Common;
namespace NovaLive.Domain.Shops;
public static class WalletStateErrors
{
    public static readonly Error InsufficientBalance = Error.Validation("Returns.InsufficientShopBalance", "Số dư khả dụng của Shop không đủ để hoàn tiền.");
}
