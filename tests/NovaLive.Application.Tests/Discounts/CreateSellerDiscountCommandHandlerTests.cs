using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Discounts;
using NovaLive.Application.UseCases.Discounts.Commands.CreateSellerDiscount;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using Xunit;

namespace NovaLive.Application.Tests.Discounts;

public class CreateSellerDiscountCommandHandlerTests
{
    private readonly Mock<IDiscountRepository> _discountRepositoryMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly CreateSellerDiscountCommandHandler _handler;
    private readonly Guid _shopId = Guid.NewGuid();

    public CreateSellerDiscountCommandHandlerTests()
    {
        _handler = new CreateSellerDiscountCommandHandler(
            _discountRepositoryMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_UnauthorizedShop_ShouldReturnUnauthorized()
    {
        _currentUserMock.Setup(u => u.ShopId).Returns((Guid?)null);

        var request = new CreateDiscountRequest(
            "CODE1", "Voucher", "PercentCart", 10m, 100_000m, 50_000m, 10, 1,
            "AllProducts", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(5), true);

        var result = await _handler.Handle(new CreateSellerDiscountCommand(request), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DiscountsErrors.UnauthorizedShop);
    }

    [Fact]
    public async Task Handle_CodeAlreadyExists_ShouldReturnConflict()
    {
        _currentUserMock.Setup(u => u.ShopId).Returns(_shopId);
        _discountRepositoryMock
            .Setup(r => r.ExistsCodeAsync("CODE1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreateDiscountRequest(
            "CODE1", "Voucher", "PercentCart", 10m, 100_000m, 50_000m, 10, 1,
            "AllProducts", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(5), true);

        var result = await _handler.Handle(new CreateSellerDiscountCommand(request), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DiscountsErrors.CodeAlreadyExists);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateDiscountAndSave()
    {
        _currentUserMock.Setup(u => u.ShopId).Returns(_shopId);
        _discountRepositoryMock
            .Setup(r => r.ExistsCodeAsync("CODE1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var now = DateTimeOffset.UtcNow;
        var request = new CreateDiscountRequest(
            "CODE1", "Khuyến mãi hè", "PercentCart", 15m, 150_000m, 50_000m, 50, 1,
            "AllProducts", null, now, now.AddDays(7), true);

        var result = await _handler.Handle(new CreateSellerDiscountCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("CODE1");
        result.Value.DiscountValue.Should().Be(15m);
        result.Value.ShopId.Should().Be(_shopId);

        _discountRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Discount>(), It.IsAny<CancellationToken>()), Times.Once);
        _discountRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
