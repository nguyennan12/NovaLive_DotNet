using FluentAssertions;
using Moq;
using NovaLive.Application.UseCases.Wallets;
using NovaLive.Application.UseCases.Wallets.Commands.ApprovePayout;
using NovaLive.Application.UseCases.Wallets.Commands.RejectPayout;
using NovaLive.Application.UseCases.Wallets.Queries.GetAdminPayouts;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.Tests.Wallets;

public sealed class AdminPayoutHandlerTests
{
    [Fact]
    public async Task ApprovePayout_ValidPendingPayout_DeductsLockedBalance_MarksCompleted_RecordsTransaction()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser("Chủ Shop A");
        var shop = await ctx.AddShop();
        var wallet = await ctx.AddWallet(shop.Id, balance: 500_000m, lockedBalance: 100_000m);
        var payout = await ctx.AddPayout(shop.Id, amount: 100_000m, status: PayoutStatus.Pending);

        var handler = new ApprovePayoutCommandHandler(
            ctx.PayoutRepository.Object,
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.ShopRepository.Object,
            ctx.UserRepository.Object,
            ctx.CurrentUser.Object,
            ctx.Clock.Object);

        var request = new ApprovePayoutRequest(TransferRef: "BANK-TXN-12345");
        var result = await handler.Handle(new ApprovePayoutCommand(payout.Id, request), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(nameof(PayoutStatus.Completed));
        result.Value.TransferRef.Should().Be("BANK-TXN-12345");

        payout.Status.Should().Be(PayoutStatus.Completed);
        payout.TransferRef.Should().Be("BANK-TXN-12345");
        wallet.LockedBalance.Should().Be(0m);
        wallet.Balance.Should().Be(500_000m);

        ctx.TransactionRepository.Verify(x => x.AddAsync(
            It.Is<ShopWalletTransaction>(t =>
                t.WalletId == wallet.Id &&
                t.Type == WalletTxType.PayoutWithdrawal &&
                t.Amount == -100_000m &&
                t.RefId == payout.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApprovePayout_PayoutNotFound_ReturnsPayoutNotFound()
    {
        using var ctx = new WalletTestContext();
        var handler = new ApprovePayoutCommandHandler(
            ctx.PayoutRepository.Object,
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.ShopRepository.Object,
            ctx.UserRepository.Object,
            ctx.CurrentUser.Object,
            ctx.Clock.Object);

        var request = new ApprovePayoutRequest(TransferRef: "BANK-TXN-12345");
        var result = await handler.Handle(new ApprovePayoutCommand(Guid.NewGuid(), request), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WalletErrors.PayoutNotFound);
    }

    [Fact]
    public async Task ApprovePayout_PayoutNotPending_ReturnsPayoutNotPending()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser();
        var shop = await ctx.AddShop();
        var wallet = await ctx.AddWallet(shop.Id, balance: 500_000m, lockedBalance: 100_000m);
        var payout = await ctx.AddPayout(shop.Id, amount: 100_000m, status: PayoutStatus.Completed);

        var handler = new ApprovePayoutCommandHandler(
            ctx.PayoutRepository.Object,
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.ShopRepository.Object,
            ctx.UserRepository.Object,
            ctx.CurrentUser.Object,
            ctx.Clock.Object);

        var request = new ApprovePayoutRequest(TransferRef: "BANK-TXN-12345");
        var result = await handler.Handle(new ApprovePayoutCommand(payout.Id, request), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WalletErrors.PayoutNotPending);
    }

    [Fact]
    public async Task RejectPayout_ValidPendingPayout_RestoresBalance_MarksFailed_RecordsTransaction()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser("Chủ Shop A");
        var shop = await ctx.AddShop();
        var wallet = await ctx.AddWallet(shop.Id, balance: 500_000m, lockedBalance: 100_000m);
        var payout = await ctx.AddPayout(shop.Id, amount: 100_000m, status: PayoutStatus.Pending);

        var handler = new RejectPayoutCommandHandler(
            ctx.PayoutRepository.Object,
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.ShopRepository.Object,
            ctx.UserRepository.Object,
            ctx.CurrentUser.Object,
            ctx.Clock.Object);

        var request = new RejectPayoutRequest(Reason: "Tên tài khoản không khớp KYC");
        var result = await handler.Handle(new RejectPayoutCommand(payout.Id, request), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(nameof(PayoutStatus.Failed));
        result.Value.Reason.Should().Be("Tên tài khoản không khớp KYC");

        payout.Status.Should().Be(PayoutStatus.Failed);
        payout.Note.Should().Be("Tên tài khoản không khớp KYC");
        wallet.LockedBalance.Should().Be(0m);
        wallet.Balance.Should().Be(600_000m);

        ctx.TransactionRepository.Verify(x => x.AddAsync(
            It.Is<ShopWalletTransaction>(t =>
                t.WalletId == wallet.Id &&
                t.Type == WalletTxType.PayoutFailedUnlock &&
                t.Amount == 100_000m &&
                t.RefId == payout.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectPayout_PayoutNotPending_ReturnsPayoutNotPending()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser();
        var shop = await ctx.AddShop();
        var wallet = await ctx.AddWallet(shop.Id, balance: 500_000m, lockedBalance: 100_000m);
        var payout = await ctx.AddPayout(shop.Id, amount: 100_000m, status: PayoutStatus.Failed);

        var handler = new RejectPayoutCommandHandler(
            ctx.PayoutRepository.Object,
            ctx.WalletRepository.Object,
            ctx.TransactionRepository.Object,
            ctx.ShopRepository.Object,
            ctx.UserRepository.Object,
            ctx.CurrentUser.Object,
            ctx.Clock.Object);

        var request = new RejectPayoutRequest(Reason: "Tên tài khoản không khớp KYC");
        var result = await handler.Handle(new RejectPayoutCommand(payout.Id, request), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WalletErrors.PayoutNotPending);
    }

    [Fact]
    public async Task GetAdminPayouts_FilteredByStatus_ReturnsPagedPayouts()
    {
        using var ctx = new WalletTestContext();
        await ctx.AddUser("Chủ Shop A");
        var shop = await ctx.AddShop();
        var payoutPending = await ctx.AddPayout(shop.Id, amount: 100_000m, status: PayoutStatus.Pending);
        var payoutCompleted = await ctx.AddPayout(shop.Id, amount: 200_000m, status: PayoutStatus.Completed);

        var handler = new GetAdminPayoutsQueryHandler(
            ctx.PayoutRepository.Object,
            ctx.ShopRepository.Object,
            ctx.UserRepository.Object);

        var request = new GetAdminPayoutsRequest(Status: nameof(PayoutStatus.Pending), Page: 1, Size: 10);
        var result = await handler.Handle(new GetAdminPayoutsQuery(request), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(1);
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].Id.Should().Be(payoutPending.Id);
        result.Value.Items[0].Status.Should().Be(nameof(PayoutStatus.Pending));
    }
}
