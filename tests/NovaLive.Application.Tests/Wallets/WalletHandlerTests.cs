using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Application.UseCases.Wallets;
using NovaLive.Application.UseCases.Wallets.Commands.CreatePayoutRequest;
using NovaLive.Application.UseCases.Wallets.Queries.GetShopWallet;
using NovaLive.Application.UseCases.Wallets.Queries.GetWalletTransactions;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.Tests.Wallets;

public sealed class WalletHandlerTests
{
    [Fact]
    public async Task GetShopWallet_ExistingWallet_ReturnsBalanceAndLockedBalance()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser();
        var shop = await ctx.AddShop();
        var wallet = await ctx.AddWallet(shop.Id, balance: 1_000_000m, lockedBalance: 200_000m);

        var handler = new GetShopWalletQueryHandler(ctx.WalletRepository.Object, ctx.Access);
        var result = await handler.Handle(new GetShopWalletQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ShopId.Should().Be(shop.Id);
        result.Value.Balance.Should().Be(1_000_000m);
        result.Value.LockedBalance.Should().Be(200_000m);
    }

    [Fact]
    public async Task GetShopWallet_NoWalletRow_ReturnsZeroBalances()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser();
        var shop = await ctx.AddShop();

        var handler = new GetShopWalletQueryHandler(ctx.WalletRepository.Object, ctx.Access);
        var result = await handler.Handle(new GetShopWalletQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ShopId.Should().Be(shop.Id);
        result.Value.Balance.Should().Be(0m);
        result.Value.LockedBalance.Should().Be(0m);
    }

    [Fact]
    public async Task GetShopWallet_PendingShop_ReturnsCannotManageError()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser();
        await ctx.AddShop(status: ShopStatus.Pending);

        var handler = new GetShopWalletQueryHandler(ctx.WalletRepository.Object, ctx.Access);
        var result = await handler.Handle(new GetShopWalletQuery(), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.CannotManage);
    }

    [Fact]
    public async Task GetWalletTransactions_ReturnsPagedTransactionsFilteredByType()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser();
        var shop = await ctx.AddShop();
        var wallet = await ctx.AddWallet(shop.Id, balance: 1_000_000m);

        var tx1 = ShopWalletTransaction.Create(
            wallet.Id,
            WalletTxType.OrderRevenue,
            500_000m,
            500_000m,
            1_000_000m,
            "Order",
            Guid.NewGuid(),
            "Doanh thu đơn hàng",
            ctx.Now.AddHours(-2));

        var tx2 = ShopWalletTransaction.Create(
            wallet.Id,
            WalletTxType.PayoutLock,
            -100_000m,
            1_000_000m,
            900_000m,
            "SellerPayout",
            Guid.NewGuid(),
            "Yêu cầu rút tiền",
            ctx.Now.AddHours(-1));

        await ctx.Set<ShopWalletTransaction>().AddRangeAsync(tx1, tx2);
        await ctx.SaveChangesAsync();

        var handler = new GetWalletTransactionsQueryHandler(
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.Access);

        var request = new GetWalletTransactionsRequest(Type: nameof(WalletTxType.PayoutLock));
        var result = await handler.Handle(new GetWalletTransactionsQuery(request), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(1);
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].Amount.Should().Be(-100_000m);
        result.Value.Items[0].Type.Should().Be(nameof(WalletTxType.PayoutLock));
    }

    [Fact]
    public async Task GetWalletTransactions_WithLocalOffsets_ConvertsToUtcAndReturnsTransactions()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser();
        var shop = await ctx.AddShop();
        var wallet = await ctx.AddWallet(shop.Id);

        var tx = ShopWalletTransaction.Create(
            wallet.Id,
            WalletTxType.PayoutLock,
            -100_000m,
            500_000m,
            400_000m,
            "SellerPayout",
            Guid.NewGuid(),
            "Rút tiền",
            ctx.Now);

        await ctx.Set<ShopWalletTransaction>().AddAsync(tx);
        await ctx.SaveChangesAsync();

        var handler = new GetWalletTransactionsQueryHandler(
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.Access);

        // Client sends +07:00 local time
        var fromLocal = new DateTimeOffset(2026, 10, 21, 10, 0, 0, TimeSpan.FromHours(7));
        var toLocal = new DateTimeOffset(2026, 10, 21, 23, 59, 59, TimeSpan.FromHours(7));

        var request = new GetWalletTransactionsRequest(From: fromLocal, To: toLocal);
        var result = await handler.Handle(new GetWalletTransactionsQuery(request), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task CreatePayoutRequest_WithoutKyc_ReturnsKycRequiredError()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser();
        var shop = await ctx.AddShop();
        await ctx.AddWallet(shop.Id, balance: 500_000m);

        var handler = new CreatePayoutRequestCommandHandler(
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.PayoutRepository.Object,
            ctx.ShopRepository.Object,
            ctx.UserRepository.Object,
            ctx.Clock.Object,
            ctx.Access);

        var command = new CreatePayoutRequestCommand(new NovaLive.Contracts.V1.Shops.CreatePayoutRequest(100_000m));
        var result = await handler.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WalletErrors.KycRequired);
    }

    [Fact]
    public async Task CreatePayoutRequest_WithPendingKyc_ReturnsKycRequiredError()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser();
        var shop = await ctx.AddShop();
        await ctx.AddKyc(shop.Id, status: VerificationStatus.Pending);
        await ctx.AddWallet(shop.Id, balance: 500_000m);

        var handler = new CreatePayoutRequestCommandHandler(
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.PayoutRepository.Object,
            ctx.ShopRepository.Object,
            ctx.UserRepository.Object,
            ctx.Clock.Object,
            ctx.Access);

        var command = new CreatePayoutRequestCommand(new NovaLive.Contracts.V1.Shops.CreatePayoutRequest(100_000m));
        var result = await handler.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WalletErrors.KycRequired);
    }

    [Fact]
    public async Task CreatePayoutRequest_ExceedsBalance_ReturnsInsufficientBalanceError()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser();
        var shop = await ctx.AddShop();
        await ctx.AddKyc(shop.Id);
        await ctx.AddWallet(shop.Id, balance: 100_000m);

        var handler = new CreatePayoutRequestCommandHandler(
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.PayoutRepository.Object,
            ctx.ShopRepository.Object,
            ctx.UserRepository.Object,
            ctx.Clock.Object,
            ctx.Access);

        var command = new CreatePayoutRequestCommand(new NovaLive.Contracts.V1.Shops.CreatePayoutRequest(200_000m));
        var result = await handler.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WalletErrors.InsufficientBalance);
    }

    [Fact]
    public async Task CreatePayoutRequest_BelowMinimumAmount_FailsValidation()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser();
        var shop = await ctx.AddShop();
        await ctx.AddKyc(shop.Id);
        await ctx.AddWallet(shop.Id, balance: 100_000m);

        var handler = new CreatePayoutRequestCommandHandler(
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.PayoutRepository.Object,
            ctx.ShopRepository.Object,
            ctx.UserRepository.Object,
            ctx.Clock.Object,
            ctx.Access);

        var command = new CreatePayoutRequestCommand(new NovaLive.Contracts.V1.Shops.CreatePayoutRequest(40_000m));
        var result = await handler.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WalletStateErrors.MinimumPayoutAmount);
    }

    [Fact]
    public async Task CreatePayoutRequest_ValidRequest_LocksBalanceAndCreatesPendingPayoutAndLedgerEntry()
    {
        using var ctx = new WalletTestContext();
        var user = await ctx.AddUser("Nguyễn Văn A");
        var shop = await ctx.AddShop();
        var kyc = await ctx.AddKyc(shop.Id);
        var wallet = await ctx.AddWallet(shop.Id, balance: 500_000m);

        var handler = new CreatePayoutRequestCommandHandler(
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.PayoutRepository.Object,
            ctx.ShopRepository.Object,
            ctx.UserRepository.Object,
            ctx.Clock.Object,
            ctx.Access);

        var amount = 150_000m;
        var command = new CreatePayoutRequestCommand(new NovaLive.Contracts.V1.Shops.CreatePayoutRequest(amount));
        var result = await handler.Handle(command, default);

        // 1. Success result
        result.IsSuccess.Should().BeTrue();
        result.Value!.Amount.Should().Be(amount);
        result.Value.BankName.Should().Be(kyc.BankName);
        result.Value.BankAccount.Should().Be(kyc.BankAccount);
        result.Value.BankHolder.Should().Be(user.FullName);
        result.Value.Status.Should().Be(nameof(PayoutStatus.Pending));

        // 2. Wallet state updated atomically
        wallet.Balance.Should().Be(350_000m);
        wallet.LockedBalance.Should().Be(150_000m);

        // 3. SellerPayout record stored
        var payout = await ctx.Set<SellerPayout>().SingleAsync();
        payout.ShopId.Should().Be(shop.Id);
        payout.Amount.Should().Be(amount);
        payout.BankAccount.Should().Be(kyc.BankAccount);
        payout.BankName.Should().Be(kyc.BankName);
        payout.Status.Should().Be(PayoutStatus.Pending);

        // 4. Ledger transaction recorded
        var tx = await ctx.Set<ShopWalletTransaction>().SingleAsync();
        tx.WalletId.Should().Be(wallet.Id);
        tx.Type.Should().Be(WalletTxType.PayoutLock);
        tx.Amount.Should().Be(-amount);
        tx.BalanceBefore.Should().Be(500_000m);
        tx.BalanceAfter.Should().Be(350_000m);
        tx.RefType.Should().Be("SellerPayout");
        tx.RefId.Should().Be(payout.Id);
    }
}
