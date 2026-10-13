using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Application.UseCases.Shops.Commands.RegisterShop;
using NovaLive.Application.UseCases.Shops.Commands.UpdateShopInfo;
using NovaLive.Application.UseCases.Shops.Commands.AddShopAddress;
using NovaLive.Application.UseCases.Shops.Commands.UpdateShopAddress;
using NovaLive.Application.UseCases.Shops.Commands.DeleteShopAddress;
using NovaLive.Application.UseCases.Shops.Commands.FollowShop;
using NovaLive.Application.UseCases.Shops.Commands.UnfollowShop;
using NovaLive.Application.UseCases.Shops.Queries.GetMyShop;
using NovaLive.Application.UseCases.Shops.Queries.GetShopPublic;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Products;
using NovaLive.Domain.System;
namespace NovaLive.Application.Tests.Shops;

public sealed class ShopWorkflowTests
{
    internal static RegisterShopRequest Registration => new("Cửa hàng Đẹp", "Description", "0912345678",
        "shop@example.com", "https://cdn.example.com/kyc-docs/front.png?signature=original",
        "https://cdn.example.com/kyc-docs/back.png", "123456789", "Bank", BankBranch: "Branch");
    internal static CreateShopAddressRequest Address => new("Warehouse", "Owner", "0912345678",
        "Province", "District", "Ward", "123 Street", false, false, 1, 2, "00003");
    private static UpdateShopProfileRequest Profile => new("Updated", "New description", null, null, "0987654321", "new@example.com");
    private static RegisterShopCommandHandler RegistrationHandler(ShopTestContext db) => new(db.App.Object, db.User.Object, db.Persistence.Object);
    private static AddShopAddressCommandHandler AddressHandler(ShopTestContext db) => new(db.App.Object, db.Access, db.Defaults);

    [Fact]
    public async Task Register_CreatesPendingShopVerificationAndSanitizedOutbox_PreservesKyc()
    {
        using var db = new ShopTestContext(); var user = await db.AddUser();
        var result = await RegistrationHandler(db).Handle(new(Registration), default);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Slug.Should().Be("cua-hang-dep");
        result.Value.Status.Should().Be("Pending");
        result.Value.BankAccount.Should().Be("******6789");
        var verification = await db.Set<ShopVerification>().SingleAsync();
        verification.ShopId.Should().Be(result.Value.Id);
        verification.SubmittedBy.Should().Be(db.UserId);
        verification.IdCardFront.Should().Be(Registration.KycFrontUrl);
        verification.IdCardBack.Should().Be(Registration.KycBackUrl);
        verification.BankBranch.Should().Be("Branch");
        verification.Status.Should().Be(VerificationStatus.Pending);
        user.IsSeller.Should().BeFalse();
        db.ChangeTracker.Entries().Should().OnlyContain(e => e.Entity is NovaLive.Domain.Users.User || e.Entity is Shop || e.Entity is ShopVerification || e.Entity is OutboxMessage);
        (await db.Set<OutboxMessage>().SingleAsync()).Payload.Should().NotContain("kyc-docs").And.NotContain(Registration.BankAccount);
    }

    [Fact]
    public async Task Register_RequiresActiveUser()
    {
        using var db = new ShopTestContext(); await db.AddUser(false);
        var result = await RegistrationHandler(db).Handle(new(Registration), default);
        result.Error.Should().Be(ShopErrors.InactiveUser);
        db.Set<Shop>().Should().BeEmpty();
    }

    [Theory]
    [InlineData(ShopStatus.Pending, false)]
    [InlineData(ShopStatus.Active, false)]
    [InlineData(ShopStatus.Suspended, false)]
    [InlineData(ShopStatus.Banned, false)]
    [InlineData(ShopStatus.Closed, false)]
    [InlineData(ShopStatus.Pending, true)]
    public async Task Register_AllExistingOwnerStatesIncludingDeleted_ReturnExact400Error(ShopStatus status, bool deleted)
    {
        using var db = new ShopTestContext(); await db.AddUser(); var shop = await db.AddShop(status: status);
        if (deleted) { db.Entry(shop).Property(s => s.DeletedAt).CurrentValue = DateTimeOffset.UtcNow; await db.SaveChangesAsync(); }
        var result = await RegistrationHandler(db).Handle(new(Registration), default);
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Message.Should().Be("Mỗi tài khoản chỉ được sở hữu tối đa 1 gian hàng");
    }

    [Fact]
    public async Task Register_NormalizedSlugCollision_Returns409()
    {
        using var db = new ShopTestContext(); await db.AddUser();
        db.Add(new Shop(Guid.NewGuid(), "Other", "cua-hang-dep")); await db.SaveChangesAsync();
        (await RegistrationHandler(db).Handle(new(Registration), default)).Error.Should().Be(ShopErrors.SlugTaken);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task Register_PersistenceRace_ReturnsMappedError(bool ownerRace)
    {
        using var db = new ShopTestContext(); await db.AddUser();
        var error = ownerRace ? ShopErrors.AlreadyOwned : ShopErrors.SlugTaken;
        db.Persistence.Setup(x => x.SaveRegistrationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(error);
        (await RegistrationHandler(db).Handle(new(Registration), default)).Error.Should().Be(error);
    }

    [Fact]
    public async Task MyShop_PendingRejectedVerification_IsVisibleOnlyToOwner()
    {
        using var db = new ShopTestContext(); var shop = await db.AddShop();
        var verification = new ShopVerification(shop.Id, db.UserId, "front", "back", null, "12345678", "Bank", null);
        db.Entry(verification).Property(v => v.Status).CurrentValue = VerificationStatus.Rejected;
        db.Entry(verification).Property(v => v.ReviewerNote).CurrentValue = "Please resubmit";
        db.Add(verification); await db.SaveChangesAsync();
        var handler = new GetMyShopQueryHandler(db.App.Object, db.Access);
        var result = await handler.Handle(new(), default);
        result.Value!.VerificationStatus.Should().Be("Rejected");
        result.Value.ReviewerNote.Should().Be("Please resubmit");
        db.User.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        (await handler.Handle(new(), default)).Error.Should().Be(ShopErrors.NotFound);
    }

    [Theory]
    [InlineData(ShopStatus.Pending, true)] [InlineData(ShopStatus.Active, true)]
    [InlineData(ShopStatus.Suspended, false)] [InlineData(ShopStatus.Banned, false)] [InlineData(ShopStatus.Closed, false)]
    public async Task Update_RespectsStatus_LeavesSlugOwnerStatusUnchanged(ShopStatus status, bool allowed)
    {
        using var db = new ShopTestContext(); var shop = await db.AddShop(status: status); var slug = shop.Slug;
        var result = await new UpdateShopInfoCommandHandler(db.App.Object, db.Access).Handle(new(Profile), default);
        result.IsSuccess.Should().Be(allowed);
        shop.Slug.Should().Be(slug); shop.OwnerId.Should().Be(db.UserId); shop.Status.Should().Be(status);
        if (!allowed) result.Error.Should().Be(ShopErrors.CannotManage);
    }

    [Fact]
    public async Task Update_DoesNotTrustTokenShopId()
    {
        using var db = new ShopTestContext(); var shop = await db.AddShop(owner: Guid.NewGuid());
        db.User.SetupGet(x => x.ShopId).Returns(shop.Id);
        (await new UpdateShopInfoCommandHandler(db.App.Object, db.Access).Handle(new(Profile), default)).Error.Should().Be(ShopErrors.NotFound);
    }

    [Fact]
    public async Task Addresses_FirstDefaults_Replacement_UpdateAndDeletePreserveExactlyOneDefault()
    {
        using var db = new ShopTestContext(); await db.AddShop();
        var first = (await AddressHandler(db).Handle(new(Address), default)).Value!; await db.SaveChangesAsync();
        first.IsPrimary.Should().BeTrue(); first.IsReturn.Should().BeTrue();
        first.ProvinceId.Should().Be(1); first.DistrictId.Should().Be(2); first.WardCode.Should().Be("00003");
        var second = (await AddressHandler(db).Handle(new(Address with { IsPrimary = true, IsReturn = true }), default)).Value!;
        await db.SaveChangesAsync();
        (await db.Set<ShopAddress>().SingleAsync(a => a.Id == first.Id)).IsPrimary.Should().BeFalse();
        (await db.Set<ShopAddress>().SingleAsync(a => a.Id == first.Id)).IsReturn.Should().BeFalse();
        await new UpdateShopAddressCommandHandler(db.App.Object, db.Access, db.Defaults)
            .Handle(new(first.Id, Address with { IsPrimary = true, IsReturn = true, DistrictId = 5 }), default);
        await db.SaveChangesAsync();
        (await db.Set<ShopAddress>().SingleAsync(a => a.Id == first.Id)).DistrictId.Should().Be(5);
        var delete = new DeleteShopAddressCommandHandler(db.App.Object, db.Access);
        (await delete.Handle(new(first.Id), default)).IsSuccess.Should().BeTrue(); await db.SaveChangesAsync();
        var remaining = await db.Set<ShopAddress>().SingleAsync();
        remaining.Id.Should().Be(second.Id); remaining.IsPrimary.Should().BeTrue(); remaining.IsReturn.Should().BeTrue();
        (await delete.Handle(new(second.Id), default)).Error.Should().Be(ShopErrors.LastPrimary);
    }

    [Fact]
    public async Task Addresses_CrossShopUpdateAndDelete_Return404()
    {
        using var db = new ShopTestContext(); await db.AddShop();
        var address = new ShopAddress(Guid.NewGuid(), "Other", "Other", "0912345678", 1, "P", 2, "D", "3", "W", "Street");
        db.Add(address); await db.SaveChangesAsync();
        (await new UpdateShopAddressCommandHandler(db.App.Object, db.Access, db.Defaults)
            .Handle(new(address.Id, Address), default)).Error.Should().Be(ShopErrors.AddressNotFound);
        (await new DeleteShopAddressCommandHandler(db.App.Object, db.Access)
            .Handle(new(address.Id), default)).Error.Should().Be(ShopErrors.AddressNotFound);
    }

    [Theory]
    [InlineData(ShopStatus.Pending)] [InlineData(ShopStatus.Suspended)]
    [InlineData(ShopStatus.Banned)] [InlineData(ShopStatus.Closed)]
    public async Task FollowAndPublic_InactiveShop_Return404(ShopStatus status)
    {
        using var db = new ShopTestContext(); var shop = await db.AddShop(Guid.NewGuid(), status);
        (await new FollowShopCommandHandler(db.App.Object, db.User.Object, db.Persistence.Object)
            .Handle(new(shop.Id), default)).Error.Should().Be(ShopErrors.NotFound);
        (await new UnfollowShopCommandHandler(db.App.Object, db.User.Object, db.Persistence.Object)
            .Handle(new(shop.Id), default)).Error.Should().Be(ShopErrors.NotFound);
        (await new GetShopPublicQueryHandler(db.App.Object, db.User.Object)
            .Handle(new(shop.Id), default)).Error.Should().Be(ShopErrors.NotFound);
    }

    [Fact]
    public async Task FollowUnfollow_AreIdempotent_AndOwnerCannotSelfFollow()
    {
        using var db = new ShopTestContext(); var shop = await db.AddShop(Guid.NewGuid(), ShopStatus.Active);
        var follow = new FollowShopCommandHandler(db.App.Object, db.User.Object, db.Persistence.Object);
        (await follow.Handle(new(shop.Id), default)).IsSuccess.Should().BeTrue();
        (await follow.Handle(new(shop.Id), default)).IsSuccess.Should().BeTrue();
        (await db.Set<ShopFollower>().CountAsync()).Should().Be(1);
        var unfollow = new UnfollowShopCommandHandler(db.App.Object, db.User.Object, db.Persistence.Object);
        (await unfollow.Handle(new(shop.Id), default)).IsSuccess.Should().BeTrue();
        (await unfollow.Handle(new(shop.Id), default)).IsSuccess.Should().BeTrue();
        (await db.Set<ShopFollower>().CountAsync()).Should().Be(0);
        var own = await db.AddShop(status: ShopStatus.Active);
        (await follow.Handle(new(own.Id), default)).Error.Should().Be(ShopErrors.SelfFollow);
    }

    [Fact]
    public async Task Public_CountsOnlyActiveProducts_ReportsFollow_AndNeverQueriesVerification()
    {
        using var db = new ShopTestContext(); var shop = await db.AddShop(Guid.NewGuid(), ShopStatus.Active);
        db.Add(new Spu(shop.Id, Guid.NewGuid(), "Active", null, null, null, null));
        db.Add(new Spu(shop.Id, Guid.NewGuid(), "Draft", null, null, null, null, ProductStatus.Draft));
        var deleted = new Spu(shop.Id, Guid.NewGuid(), "Deleted", null, null, null, null); deleted.SoftDelete(); db.Add(deleted);
        db.Add(new ShopFollower(shop.Id, db.UserId)); await db.SaveChangesAsync();
        var handler = new GetShopPublicQueryHandler(db.App.Object, db.User.Object);
        var result = (await handler.Handle(new(shop.Id), default)).Value!;
        result.ProductsCount.Should().Be(1); result.FollowersCount.Should().Be(1); result.IsFollowing.Should().BeTrue();
        db.App.VerifyGet(x => x.ShopVerifications, Times.Never);
        typeof(ShopDetailResponse).GetProperties().Select(p => p.Name).Should()
            .NotContain(new[] { "BankAccount", "BankName", "IdCardFront", "IdCardBack", "ReviewerNote", "BusinessLicense" });
        db.User.SetupGet(u => u.IsAuthenticated).Returns(false);
        (await handler.Handle(new(shop.Id), default)).Value!.IsFollowing.Should().BeFalse();
        db.Entry(shop).Property(s => s.DeletedAt).CurrentValue = DateTimeOffset.UtcNow; await db.SaveChangesAsync();
        (await handler.Handle(new(shop.Id), default)).Error.Should().Be(ShopErrors.NotFound);
    }
}
