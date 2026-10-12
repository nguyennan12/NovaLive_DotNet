using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Discounts;
using NovaLive.Application.UseCases.Discounts.Commands.UpdateDiscount;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using Xunit;

namespace NovaLive.Application.Tests.Discounts;

public class UpdateDiscountCommandHandlerTests
{
    private readonly Mock<IDiscountRepository> _discountRepositoryMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly UpdateDiscountCommandHandler _handler;
    private readonly Guid _shopId = Guid.NewGuid();

    public UpdateDiscountCommandHandlerTests()
    {
        _handler = new UpdateDiscountCommandHandler(
            _discountRepositoryMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnNotFound()
    {
        var id = Guid.NewGuid();
        _discountRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Discount?)null);

        var request = new UpdateDiscountRequest("New Name", 100, DateTimeOffset.UtcNow.AddDays(10), true);
        var result = await _handler.Handle(new UpdateDiscountCommand(id, request), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DiscountsErrors.NotFound);
    }

    [Fact]
    public async Task Handle_DifferentShop_ShouldReturnForbiddenShop()
    {
        var id = Guid.NewGuid();
        var otherShopId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var discount = Discount.CreateShopDiscount(
            otherShopId, "CODE1", "Voucher", DiscountType.FixedCart, 20_000m,
            0m, null, 10, 1, DiscountAppliesTo.AllProducts, null, now, now.AddDays(5), true).Value!;

        _currentUserMock.Setup(u => u.ShopId).Returns(_shopId);
        _discountRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        var request = new UpdateDiscountRequest("New Name", 100, now.AddDays(10), true);
        var result = await _handler.Handle(new UpdateDiscountCommand(id, request), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DiscountsErrors.ForbiddenShop);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldUpdateAndSave()
    {
        var id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var discount = Discount.CreateShopDiscount(
            _shopId, "CODE1", "Old Name", DiscountType.FixedCart, 20_000m,
            0m, null, 10, 1, DiscountAppliesTo.AllProducts, null, now, now.AddDays(5), true).Value!;

        _currentUserMock.Setup(u => u.ShopId).Returns(_shopId);
        _discountRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        var newValidTo = now.AddDays(15);
        var request = new UpdateDiscountRequest("Updated Name", 25, newValidTo, false);
        var result = await _handler.Handle(new UpdateDiscountCommand(id, request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Updated Name");
        result.Value.MaxUses.Should().Be(25);
        result.Value.IsActive.Should().BeFalse();

        _discountRepositoryMock.Verify(r => r.Update(discount), Times.Once);
        _discountRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
