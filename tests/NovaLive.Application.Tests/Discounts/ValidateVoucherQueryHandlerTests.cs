using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Discounts.Queries.ValidateVoucher;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Carts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Products;
using Xunit;

namespace NovaLive.Application.Tests.Discounts;

public class ValidateVoucherQueryHandlerTests
{
    private readonly Mock<IDiscountRepository> _discountRepositoryMock = new();
    private readonly Mock<ICartRepository> _cartRepositoryMock = new();
    private readonly Mock<ISkuRepository> _skuRepositoryMock = new();
    private readonly Mock<ISpuRepository> _spuRepositoryMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly ValidateVoucherQueryHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _shopId = Guid.NewGuid();
    private readonly DateTimeOffset _currentTime = new(2026, 10, 12, 12, 0, 0, TimeSpan.Zero);

    public ValidateVoucherQueryHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(_currentTime);

        _handler = new ValidateVoucherQueryHandler(
            _discountRepositoryMock.Object,
            _cartRepositoryMock.Object,
            _skuRepositoryMock.Object,
            _spuRepositoryMock.Object,
            _currentUserMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_VoucherNotFound_ShouldReturnInvalid()
    {
        _discountRepositoryMock
            .Setup(r => r.GetByCodeAsync("UNKNOWN", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Discount?)null);

        var request = new ValidateVoucherRequest("UNKNOWN", _shopId, [Guid.NewGuid()]);
        var result = await _handler.Handle(new ValidateVoucherQuery(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsValid.Should().BeFalse();
        result.Value.ReasonIfInvalid.Should().Contain("không tồn tại");
    }

    [Fact]
    public async Task Handle_VoucherMaxUsesReached_ShouldReturnInvalid()
    {
        var discount = Discount.CreateShopDiscount(
            _shopId, "MAXOUT", "Voucher", DiscountType.FixedCart, 10_000m,
            0m, null, 1, 1, DiscountAppliesTo.AllProducts, null,
            _currentTime.AddDays(-1), _currentTime.AddDays(5), true).Value!;

        discount.IncrementUsedCount(); // UsedCount = 1 == MaxUses

        _discountRepositoryMock
            .Setup(r => r.GetByCodeAsync("MAXOUT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        var request = new ValidateVoucherRequest("MAXOUT", _shopId, [Guid.NewGuid()]);
        var result = await _handler.Handle(new ValidateVoucherQuery(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsValid.Should().BeFalse();
        result.Value.ReasonIfInvalid.Should().Contain("hết lượt sử dụng trên hệ thống");
    }

    [Fact]
    public async Task Handle_PerUserLimitReached_ShouldReturnInvalid()
    {
        var discount = Discount.CreateShopDiscount(
            _shopId, "ONCEONLY", "Voucher", DiscountType.FixedCart, 10_000m,
            0m, null, 100, 1, DiscountAppliesTo.AllProducts, null,
            _currentTime.AddDays(-1), _currentTime.AddDays(5), true).Value!;

        _discountRepositoryMock
            .Setup(r => r.GetByCodeAsync("ONCEONLY", It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        _discountRepositoryMock
            .Setup(r => r.GetUserUsageCountAsync(discount.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); // Already used 1 == perUserLimit

        var request = new ValidateVoucherRequest("ONCEONLY", _shopId, [Guid.NewGuid()]);
        var result = await _handler.Handle(new ValidateVoucherQuery(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsValid.Should().BeFalse();
        result.Value.ReasonIfInvalid.Should().Contain("hết lượt cho phép của mã voucher này");
    }

    [Fact]
    public async Task Handle_ValidShopVoucher_ShouldCalculateDiscountUsingRealTimeSkuPrice()
    {
        var discount = Discount.CreateShopDiscount(
            _shopId, "SHOP10", "Giảm 10%", DiscountType.PercentCart, 10m,
            100_000m, 50_000m, 100, 2, DiscountAppliesTo.AllProducts, null,
            _currentTime.AddDays(-1), _currentTime.AddDays(5), true).Value!;

        _discountRepositoryMock
            .Setup(r => r.GetByCodeAsync("SHOP10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        _discountRepositoryMock
            .Setup(r => r.GetUserUsageCountAsync(discount.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var cartItemId = Guid.NewGuid();
        var skuId = Guid.NewGuid();
        var spu = new Spu(_shopId, Guid.NewGuid(), "SPU Test", null, null, null, null);
        var sku = new Sku(spu.Id, _shopId, "SKU1", null, 120_000m, 100_000m, 200, true);

        // Cart item with quantity 2
        var cartItem = new CartItem(cartItemId, Guid.NewGuid(), sku.Id, _shopId, 2, 80_000m);

        _cartRepositoryMock
            .Setup(r => r.GetCartItemsByIdsAsync(It.IsAny<IEnumerable<Guid>>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CartItem> { cartItem });

        _skuRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Sku> { sku });

        _spuRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Spu> { spu });

        var request = new ValidateVoucherRequest("SHOP10", _shopId, [cartItemId]);
        var result = await _handler.Handle(new ValidateVoucherQuery(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsValid.Should().BeTrue();
        // 2 items * 100_000 (real-time sellPrice) = 200_000; 10% of 200_000 = 20_000
        result.Value.DiscountAmount.Should().Be(20_000m);
        result.Value.ReasonIfInvalid.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MinOrderAmountNotMet_ShouldReturnInvalid()
    {
        var discount = Discount.CreateShopDiscount(
            _shopId, "SHOP10", "Giảm 10%", DiscountType.PercentCart, 10m,
            300_000m, 50_000m, 100, 2, DiscountAppliesTo.AllProducts, null,
            _currentTime.AddDays(-1), _currentTime.AddDays(5), true).Value!;

        _discountRepositoryMock
            .Setup(r => r.GetByCodeAsync("SHOP10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        _discountRepositoryMock
            .Setup(r => r.GetUserUsageCountAsync(discount.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var cartItemId = Guid.NewGuid();
        var spu = new Spu(_shopId, Guid.NewGuid(), "SPU Test", null, null, null, null);
        var sku = new Sku(spu.Id, _shopId, "SKU1", null, 120_000m, 100_000m, 200, true);
        var cartItem = new CartItem(cartItemId, Guid.NewGuid(), sku.Id, _shopId, 2, 80_000m);

        _cartRepositoryMock
            .Setup(r => r.GetCartItemsByIdsAsync(It.IsAny<IEnumerable<Guid>>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CartItem> { cartItem });

        _skuRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Sku> { sku });

        _spuRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Spu> { spu });

        var request = new ValidateVoucherRequest("SHOP10", _shopId, [cartItemId]);
        var result = await _handler.Handle(new ValidateVoucherQuery(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsValid.Should().BeFalse();
        result.Value.DiscountAmount.Should().Be(0m);
        result.Value.ReasonIfInvalid.Should().Contain("chưa đạt mức tối thiểu");
    }
}
