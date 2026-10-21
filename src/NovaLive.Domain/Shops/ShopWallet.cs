using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class ShopWallet : Entity
{
    private ShopWallet()
    {
    }

    public ShopWallet(Guid shopId)
    {
        ShopId = shopId;
    }

    public Guid ShopId { get; private set; }

    public decimal Balance { get; private set; }

    public decimal LockedBalance { get; private set; }

    public string Currency { get; private set; } = "VND";

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public void Credit(decimal amount, DateTimeOffset now)
    {
        Balance += amount;
        UpdatedAt = now;
    }

    public Result DebitRefund(decimal amount, DateTimeOffset now)
    {
        if (amount < 0 || Balance < amount) return WalletStateErrors.InsufficientBalance;
        Balance -= amount; UpdatedAt = now;
        return Result.Success();
    }

    public Result LockPayout(decimal amount, DateTimeOffset now)
    {
        if (amount < 50_000m) return WalletStateErrors.MinimumPayoutAmount;
        if (Balance < amount) return WalletStateErrors.InsufficientPayoutBalance;
        Balance -= amount;
        LockedBalance += amount;
        UpdatedAt = now;
        return Result.Success();
    }

    public Result UnlockPayout(decimal amount, DateTimeOffset now)
    {
        if (amount < 0 || LockedBalance < amount) return WalletStateErrors.InsufficientLockedBalance;
        LockedBalance -= amount;
        Balance += amount;
        UpdatedAt = now;
        return Result.Success();
    }

    public Result CompletePayout(decimal amount, DateTimeOffset now)
    {
        if (amount < 0 || LockedBalance < amount) return WalletStateErrors.InsufficientLockedBalance;
        LockedBalance -= amount;
        UpdatedAt = now;
        return Result.Success();
    }
}
