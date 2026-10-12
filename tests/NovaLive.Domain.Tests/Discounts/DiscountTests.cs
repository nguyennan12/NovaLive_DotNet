using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using Xunit;

namespace NovaLive.Domain.Tests.Discounts;

public class DiscountTests
{
    private readonly Guid _testShopId = Guid.NewGuid();

    [Fact]
    public void CreateShopDiscount_ValidInput_ShouldSucceed()
    {
        var now = DateTimeOffset.UtcNow;
        var validFrom = now.AddDays(1);
        var validTo = now.AddDays(7);

        var result = Discount.CreateShopDiscount(
            shopId: _testShopId,
            code: "shop10",
            name: "Giảm 10% đơn hàng",
            discountType: DiscountType.PercentCart,
            discountValue: 10m,
            minOrderAmount: 100_000m,
            maxDiscountAmount: 50_000m,
            maxUses: 100,
            perUserLimit: 1,
            appliesTo: DiscountAppliesTo.AllProducts,
            targetIds: null,
            validFrom: validFrom,
            validTo: validTo,
            isPublic: true);

        result.IsSuccess.Should().BeTrue();
        var discount = result.Value!;
        discount.ShopId.Should().Be(_testShopId);
        discount.Code.Should().Be("SHOP10");
        discount.DiscountType.Should().Be(DiscountType.PercentCart);
        discount.DiscountValue.Should().Be(10m);
        discount.UsedCount.Should().Be(0);
        discount.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(DiscountType.FreeShip)]
    [InlineData(DiscountType.PercentShip)]
    public void CreateShopDiscount_InvalidType_ShouldFail(DiscountType invalidType)
    {
        var now = DateTimeOffset.UtcNow;
        var result = Discount.CreateShopDiscount(
            shopId: _testShopId,
            code: "SHIPDISC",
            name: "Ship Voucher",
            discountType: invalidType,
            discountValue: 10m,
            minOrderAmount: 0m,
            maxDiscountAmount: 50_000m,
            maxUses: 100,
            perUserLimit: 1,
            appliesTo: DiscountAppliesTo.AllProducts,
            targetIds: null,
            validFrom: now,
            validTo: now.AddDays(1),
            isPublic: true);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Discounts.InvalidType");
    }

    [Fact]
    public void CreateShopDiscount_ValidToBeforeValidFrom_ShouldFail()
    {
        var now = DateTimeOffset.UtcNow;
        var result = Discount.CreateShopDiscount(
            shopId: _testShopId,
            code: "SHOP10",
            name: "Voucher",
            discountType: DiscountType.PercentCart,
            discountValue: 10m,
            minOrderAmount: 0m,
            maxDiscountAmount: 10_000m,
            maxUses: 10,
            perUserLimit: 1,
            appliesTo: DiscountAppliesTo.AllProducts,
            targetIds: null,
            validFrom: now.AddDays(2),
            validTo: now.AddDays(1),
            isPublic: true);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Discounts.InvalidDateRange");
    }

    [Fact]
    public void CreateShopDiscount_PercentWithoutMaxDiscount_ShouldFail()
    {
        var now = DateTimeOffset.UtcNow;
        var result = Discount.CreateShopDiscount(
            shopId: _testShopId,
            code: "SHOP10",
            name: "Voucher",
            discountType: DiscountType.PercentCart,
            discountValue: 10m,
            minOrderAmount: 0m,
            maxDiscountAmount: null,
            maxUses: 10,
            perUserLimit: 1,
            appliesTo: DiscountAppliesTo.AllProducts,
            targetIds: null,
            validFrom: now,
            validTo: now.AddDays(1),
            isPublic: true);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Discounts.MaxDiscountAmountRequired");
    }

    [Fact]
    public void CreatePlatformDiscount_ValidInput_ShouldHaveNullShopId()
    {
        var now = DateTimeOffset.UtcNow;
        var result = Discount.CreatePlatformDiscount(
            code: "FREESHIP",
            name: "Miễn phí vận chuyển toàn sàn",
            discountType: DiscountType.FreeShip,
            discountValue: 30_000m,
            minOrderAmount: 200_000m,
            maxDiscountAmount: null,
            maxUses: 500,
            perUserLimit: 2,
            appliesTo: DiscountAppliesTo.AllProducts,
            targetIds: null,
            validFrom: now,
            validTo: now.AddDays(30),
            isPublic: true);

        result.IsSuccess.Should().BeTrue();
        var discount = result.Value!;
        discount.ShopId.Should().BeNull();
        discount.Code.Should().Be("FREESHIP");
        discount.DiscountType.Should().Be(DiscountType.FreeShip);
    }

    [Fact]
    public void Update_ValidInput_ShouldUpdateState()
    {
        var now = DateTimeOffset.UtcNow;
        var discount = Discount.CreateShopDiscount(
            _testShopId, "SHOP10", "Old Name", DiscountType.FixedCart,
            20_000m, 50_000m, null, 10, 1, DiscountAppliesTo.AllProducts, null,
            now, now.AddDays(5), true).Value!;

        var newValidTo = now.AddDays(10);
        var updateResult = discount.Update("New Name", 20, newValidTo, false);

        updateResult.IsSuccess.Should().BeTrue();
        discount.Name.Should().Be("New Name");
        discount.MaxUses.Should().Be(20);
        discount.ValidTo.Should().Be(newValidTo);
        discount.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Update_MaxUsesLessThanUsedCount_ShouldFail()
    {
        var now = DateTimeOffset.UtcNow;
        var discount = Discount.CreateShopDiscount(
            _testShopId, "SHOP10", "Name", DiscountType.FixedCart,
            20_000m, 50_000m, null, 10, 1, DiscountAppliesTo.AllProducts, null,
            now, now.AddDays(5), true).Value!;

        discount.IncrementUsedCount();
        discount.IncrementUsedCount();
        discount.IncrementUsedCount(); // UsedCount = 3

        var updateResult = discount.Update("Name", 2, now.AddDays(5), true);

        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Code.Should().Be("Discounts.InvalidMaxUses");
    }
}
