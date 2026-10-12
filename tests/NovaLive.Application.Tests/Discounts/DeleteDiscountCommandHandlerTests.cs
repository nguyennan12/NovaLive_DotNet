using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Discounts;
using NovaLive.Application.UseCases.Discounts.Commands.DeletePlatformDiscount;
using NovaLive.Application.UseCases.Discounts.Commands.DeleteSellerDiscount;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using Xunit;

namespace NovaLive.Application.Tests.Discounts;

public class DeleteDiscountCommandHandlerTests
{
    private readonly Mock<IDiscountRepository> _discountRepositoryMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();

    [Fact]
    public async Task DeleteSellerDiscount_ShouldReturnCannotDeleteUsedVoucher_WhenUsedCountGreaterThanZero()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var discountId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.ShopId).Returns(shopId);

        var discount = Discount.CreateShopDiscount(
            shopId: shopId,
            code: "SHOP10",
            name: "Shop 10%",
            discountType: DiscountType.PercentCart,
            discountValue: 10,
            minOrderAmount: 100_000m,
            maxDiscountAmount: 50_000m,
            maxUses: 100,
            perUserLimit: 1,
            appliesTo: DiscountAppliesTo.AllProducts,
            targetIds: null,
            validFrom: DateTimeOffset.UtcNow.AddDays(-1),
            validTo: DateTimeOffset.UtcNow.AddDays(5),
            isPublic: true).Value!;

        discount.IncrementUsedCount(); // UsedCount = 1

        _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        var handler = new DeleteSellerDiscountCommandHandler(_discountRepositoryMock.Object, _currentUserMock.Object);

        // Act
        var result = await handler.Handle(new DeleteSellerDiscountCommand(discountId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DiscountsErrors.CannotDeleteUsedVoucher);
        _discountRepositoryMock.Verify(r => r.Delete(It.IsAny<Discount>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSellerDiscount_ShouldSucceed_WhenUsedCountIsZero()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var discountId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.ShopId).Returns(shopId);

        var discount = Discount.CreateShopDiscount(
            shopId: shopId,
            code: "SHOP10",
            name: "Shop 10%",
            discountType: DiscountType.PercentCart,
            discountValue: 10,
            minOrderAmount: 100_000m,
            maxDiscountAmount: 50_000m,
            maxUses: 100,
            perUserLimit: 1,
            appliesTo: DiscountAppliesTo.AllProducts,
            targetIds: null,
            validFrom: DateTimeOffset.UtcNow.AddDays(-1),
            validTo: DateTimeOffset.UtcNow.AddDays(5),
            isPublic: true).Value!;

        _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        var handler = new DeleteSellerDiscountCommandHandler(_discountRepositoryMock.Object, _currentUserMock.Object);

        // Act
        var result = await handler.Handle(new DeleteSellerDiscountCommand(discountId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _discountRepositoryMock.Verify(r => r.Delete(discount), Times.Once);
        _discountRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePlatformDiscount_ShouldReturnCannotDeleteUsedVoucher_WhenUsedCountGreaterThanZero()
    {
        // Arrange
        var discountId = Guid.NewGuid();

        var discount = Discount.CreatePlatformDiscount(
            code: "PLATFORM30",
            name: "Platform 30k",
            discountType: DiscountType.FixedCart,
            discountValue: 30_000m,
            minOrderAmount: 50_000m,
            maxDiscountAmount: null,
            maxUses: 100,
            perUserLimit: 1,
            appliesTo: DiscountAppliesTo.AllProducts,
            targetIds: null,
            validFrom: DateTimeOffset.UtcNow.AddDays(-1),
            validTo: DateTimeOffset.UtcNow.AddDays(5),
            isPublic: true).Value!;

        discount.IncrementUsedCount(); // UsedCount = 1

        _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        var handler = new DeletePlatformDiscountCommandHandler(_discountRepositoryMock.Object);

        // Act
        var result = await handler.Handle(new DeletePlatformDiscountCommand(discountId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DiscountsErrors.CannotDeleteUsedVoucher);
        _discountRepositoryMock.Verify(r => r.Delete(It.IsAny<Discount>()), Times.Never);
    }

    [Fact]
    public async Task DeletePlatformDiscount_ShouldSucceed_WhenUsedCountIsZero()
    {
        // Arrange
        var discountId = Guid.NewGuid();

        var discount = Discount.CreatePlatformDiscount(
            code: "PLATFORM30",
            name: "Platform 30k",
            discountType: DiscountType.FixedCart,
            discountValue: 30_000m,
            minOrderAmount: 50_000m,
            maxDiscountAmount: null,
            maxUses: 100,
            perUserLimit: 1,
            appliesTo: DiscountAppliesTo.AllProducts,
            targetIds: null,
            validFrom: DateTimeOffset.UtcNow.AddDays(-1),
            validTo: DateTimeOffset.UtcNow.AddDays(5),
            isPublic: true).Value!;

        _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        var handler = new DeletePlatformDiscountCommandHandler(_discountRepositoryMock.Object);

        // Act
        var result = await handler.Handle(new DeletePlatformDiscountCommand(discountId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _discountRepositoryMock.Verify(r => r.Delete(discount), Times.Once);
        _discountRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
