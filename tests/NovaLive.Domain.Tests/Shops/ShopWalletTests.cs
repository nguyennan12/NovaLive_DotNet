using FluentAssertions;
using NovaLive.Domain.Shops;

namespace NovaLive.Domain.Tests.Shops;

public sealed class ShopWalletTests
{
    [Fact]
    public void LockPayout_BelowMinimumAmount_Fails()
    {
        var wallet = new ShopWallet(Guid.NewGuid());
        wallet.Credit(100_000m, DateTimeOffset.UtcNow);

        var result = wallet.LockPayout(49_999m, DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WalletStateErrors.MinimumPayoutAmount);
        wallet.Balance.Should().Be(100_000m);
        wallet.LockedBalance.Should().Be(0m);
    }

    [Fact]
    public void LockPayout_InsufficientBalance_Fails()
    {
        var wallet = new ShopWallet(Guid.NewGuid());
        wallet.Credit(50_000m, DateTimeOffset.UtcNow);

        var result = wallet.LockPayout(60_000m, DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WalletStateErrors.InsufficientPayoutBalance);
        wallet.Balance.Should().Be(50_000m);
        wallet.LockedBalance.Should().Be(0m);
    }

    [Fact]
    public void LockPayout_ValidAmount_TransfersBalanceToLockedBalance()
    {
        var wallet = new ShopWallet(Guid.NewGuid());
        wallet.Credit(200_000m, DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow.AddMinutes(5);

        var result = wallet.LockPayout(150_000m, now);

        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Should().Be(50_000m);
        wallet.LockedBalance.Should().Be(150_000m);
        wallet.UpdatedAt.Should().Be(now);
    }

    [Fact]
    public void UnlockPayout_ExceedsLockedBalance_Fails()
    {
        var wallet = new ShopWallet(Guid.NewGuid());
        wallet.Credit(100_000m, DateTimeOffset.UtcNow);
        wallet.LockPayout(100_000m, DateTimeOffset.UtcNow);

        var result = wallet.UnlockPayout(100_001m, DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WalletStateErrors.InsufficientLockedBalance);
    }

    [Fact]
    public void UnlockPayout_ValidAmount_RestoresBalanceFromLockedBalance()
    {
        var wallet = new ShopWallet(Guid.NewGuid());
        wallet.Credit(200_000m, DateTimeOffset.UtcNow);
        wallet.LockPayout(100_000m, DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow.AddMinutes(10);

        var result = wallet.UnlockPayout(100_000m, now);

        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Should().Be(200_000m);
        wallet.LockedBalance.Should().Be(0m);
        wallet.UpdatedAt.Should().Be(now);
    }

    [Fact]
    public void CompletePayout_ValidAmount_DeductsLockedBalance()
    {
        var wallet = new ShopWallet(Guid.NewGuid());
        wallet.Credit(200_000m, DateTimeOffset.UtcNow);
        wallet.LockPayout(100_000m, DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow.AddMinutes(10);

        var result = wallet.CompletePayout(100_000m, now);

        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Should().Be(100_000m);
        wallet.LockedBalance.Should().Be(0m);
        wallet.UpdatedAt.Should().Be(now);
    }
}
