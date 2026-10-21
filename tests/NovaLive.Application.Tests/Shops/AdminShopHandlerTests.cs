using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Application.UseCases.Shops.Commands.ApproveShop;
using NovaLive.Application.UseCases.Shops.Commands.BanShop;
using NovaLive.Application.UseCases.Shops.Queries.GetAdminShops;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;
using NovaLive.Domain.Users;

namespace NovaLive.Application.Tests.Shops;

public sealed class AdminShopHandlerTests
{
    [Fact]
    public async Task ApproveShop_WithoutKycVerification_ReturnsValidationErrorAndDoesNotActivateShop()
    {
        using var db = new ShopTestContext();
        var user = await db.AddUser();
        var shop = await db.AddShop(user.Id, status: ShopStatus.Pending);

        var walletMock = new Mock<IShopWalletRepository>();
        var dbContextMock = new Mock<IAppDbContext>();
        var clock = new Mock<IDateTimeProvider>();
        var handler = new ApproveShopCommandHandler(
            db.Repository.Object,
            db.Users.Object,
            walletMock.Object,
            dbContextMock.Object,
            db.User.Object,
            clock.Object);

        var result = await handler.Handle(
            new ApproveShopCommand(shop.Id, new ApproveShopRequest(Note: null)), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.VerificationNotPending);
        shop.Status.Should().Be(ShopStatus.Pending);
        walletMock.Verify(w => w.AddAsync(It.IsAny<ShopWallet>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveShop_ValidRequest_ApprovesShopAndKyc_GrantsSellerRole_InitializesWallet()
    {
        using var db = new ShopTestContext();
        var user = await db.AddUser();
        var shop = await db.AddShop(user.Id, status: ShopStatus.Pending);

        var verification = new ShopVerification(
            shop.Id,
            user.Id,
            "front.png",
            "back.png",
            null,
            "0987654321",
            "VietinBank",
            "HN");
        await db.Set<ShopVerification>().AddAsync(verification);
        await db.SaveChangesAsync();

        var walletMock = new Mock<IShopWalletRepository>();
        ShopWallet? createdWallet = null;
        walletMock.Setup(w => w.GetByShopIdAsync(shop.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShopWallet?)null);
        walletMock.Setup(w => w.AddAsync(It.IsAny<ShopWallet>(), It.IsAny<CancellationToken>()))
            .Callback<ShopWallet, CancellationToken>((w, _) => createdWallet = w)
            .Returns(Task.CompletedTask);

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock.SetupGet(x => x.UserRoles).Returns(db.UserRoles);

        var clock = new Mock<IDateTimeProvider>();
        var now = new DateTimeOffset(2026, 10, 21, 14, 0, 0, TimeSpan.Zero);
        clock.SetupGet(c => c.UtcNow).Returns(now);

        var handler = new ApproveShopCommandHandler(
            db.Repository.Object,
            db.Users.Object,
            walletMock.Object,
            dbContextMock.Object,
            db.User.Object,
            clock.Object);

        var request = new ApproveShopRequest(Note: "Hồ sơ hợp lệ");
        var result = await handler.Handle(new ApproveShopCommand(shop.Id, request), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(nameof(ShopStatus.Active));
        result.Value.VerificationStatus.Should().Be(nameof(VerificationStatus.Approved));
        result.Value.ReviewerNote.Should().Be("Hồ sơ hợp lệ");

        shop.Status.Should().Be(ShopStatus.Active);
        verification.Status.Should().Be(VerificationStatus.Approved);
        verification.ReviewerId.Should().Be(db.UserId);
        verification.ReviewerNote.Should().Be("Hồ sơ hợp lệ");
        verification.ReviewedAt.Should().Be(now);

        user.IsSeller.Should().BeTrue();
        await db.SaveChangesAsync();
        db.UserRoles.Should().ContainSingle(r => r.UserId == user.Id && r.RoleId == SystemRoleIds.Seller);

        createdWallet.Should().NotBeNull();
        createdWallet!.ShopId.Should().Be(shop.Id);
        createdWallet.Balance.Should().Be(0m);
        createdWallet.LockedBalance.Should().Be(0m);
    }

    [Fact]
    public async Task ApproveShop_ShopNotPending_ReturnsNotPending()
    {
        using var db = new ShopTestContext();
        var user = await db.AddUser();
        var shop = await db.AddShop(user.Id, status: ShopStatus.Banned);

        var walletMock = new Mock<IShopWalletRepository>();
        var dbContextMock = new Mock<IAppDbContext>();
        var clock = new Mock<IDateTimeProvider>();

        var handler = new ApproveShopCommandHandler(
            db.Repository.Object,
            db.Users.Object,
            walletMock.Object,
            dbContextMock.Object,
            db.User.Object,
            clock.Object);

        var request = new ApproveShopRequest(Note: "Hồ sơ");
        var result = await handler.Handle(new ApproveShopCommand(shop.Id, request), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.NotPending);
    }

    [Fact]
    public async Task BanShop_ValidRequest_BansShopAndDeactivatesAllProductsAndRecordsAuditLog()
    {
        using var db = new ShopTestContext();
        var user = await db.AddUser();
        var shop = await db.AddShop(user.Id, status: ShopStatus.Active);

        var spu1 = new Spu(shop.Id, Guid.NewGuid(), "Áo thun", null, null, null, null, ProductStatus.Active);
        var spu2 = new Spu(shop.Id, Guid.NewGuid(), "Quần jean", null, null, null, null, ProductStatus.Active);
        await db.Set<Spu>().AddRangeAsync(spu1, spu2);
        await db.SaveChangesAsync();

        var handler = new BanShopCommandHandler(
            db.Repository.Object,
            db.Products.Object,
            db.AuditLogs.Object,
            db.User.Object,
            db.Clock.Object);

        var request = new BanShopRequest(Reason: "Vi phạm chính sách nền tảng");
        var result = await handler.Handle(new BanShopCommand(shop.Id, request), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(nameof(ShopStatus.Banned));
        shop.Status.Should().Be(ShopStatus.Banned);

        var updatedSpu1 = await db.Set<Spu>().FindAsync(spu1.Id);
        var updatedSpu2 = await db.Set<Spu>().FindAsync(spu2.Id);
        updatedSpu1!.Status.Should().Be(ProductStatus.Inactive);
        updatedSpu2!.Status.Should().Be(ProductStatus.Inactive);

        db.AuditLogs.Verify(x => x.AddAsync(
            It.Is<AuditLog>(l => l.Action == "Shop.Banned" && l.EntityId == shop.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BanShop_AlreadyBannedShop_ReturnsAlreadyBanned()
    {
        using var db = new ShopTestContext();
        var user = await db.AddUser();
        var shop = await db.AddShop(user.Id, status: ShopStatus.Banned);

        var handler = new BanShopCommandHandler(
            db.Repository.Object,
            db.Products.Object,
            db.AuditLogs.Object,
            db.User.Object,
            db.Clock.Object);

        var request = new BanShopRequest(Reason: "Vi phạm tiếp");
        var result = await handler.Handle(new BanShopCommand(shop.Id, request), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.AlreadyBanned);
    }

    [Fact]
    public async Task GetAdminShops_FilterByStatus_ReturnsPagedShops()
    {
        using var db = new ShopTestContext();
        var user1 = await db.AddUser();
        var shop1 = await db.AddShop(user1.Id, status: ShopStatus.Pending);

        var user2 = new User("seller2@example.com", "hash", "Seller 2", "0912345679");
        db.Set<User>().Add(user2);
        await db.SaveChangesAsync();
        var shop2 = await db.AddShop(user2.Id, status: ShopStatus.Active);

        var handler = new GetAdminShopsQueryHandler(db.Repository.Object);

        var request = new GetAdminShopsRequest(Status: nameof(ShopStatus.Pending), Page: 1, Size: 10);
        var result = await handler.Handle(new GetAdminShopsQuery(request), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(1);
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].Id.Should().Be(shop1.Id);
        result.Value.Items[0].Status.Should().Be(nameof(ShopStatus.Pending));
    }
}
