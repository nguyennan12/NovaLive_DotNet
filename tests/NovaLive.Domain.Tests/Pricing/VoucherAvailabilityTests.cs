using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Pricing;

namespace NovaLive.Domain.Tests.Pricing;

public sealed class VoucherAvailabilityTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 15, 0, 0, 0, TimeSpan.Zero);

    private static Discount Voucher(Guid? shopId = null, DiscountType type = DiscountType.FixedCart, bool active = true) =>
        new(shopId, "CODE", "Voucher", type, 10m, 100m, null, 1, 1,
            DiscountAppliesTo.AllProducts, null, Now.AddHours(-1), Now.AddHours(1), true, active);

    [Fact]
    public void Availability_RespectsInclusiveTimeBoundaries_WithoutChangingUsageOrEntity()
    {
        var voucher = Voucher(); var updatedAt = voucher.UpdatedAt;
        foreach (var time in new[] { voucher.ValidFrom, Now, voucher.ValidTo })
            VoucherEligibilityEvaluator.EvaluateAvailability(voucher, 0, null, false, time).IsSuccess.Should().BeTrue();
        voucher.UsedCount.Should().Be(0); voucher.UpdatedAt.Should().Be(updatedAt);
    }

    [Theory]
    [InlineData("global-limit", "Voucher.UsesExhausted")]
    [InlineData("user-limit", "Voucher.UserLimitReached")]
    [InlineData("scope", "Pricing.PlatformScopeMismatch")]
    [InlineData("slot", "Pricing.InvalidVoucherTypeForSlot")]
    [InlineData("inactive", "Voucher.Inactive")]
    public void Availability_RejectsUsageAndSlotViolations(string scenario, string errorCode)
    {
        var voucher = Voucher(scenario == "scope" ? Guid.NewGuid() : null,
            scenario == "slot" ? DiscountType.FreeShip : DiscountType.FixedCart, scenario != "inactive");
        if (scenario == "global-limit") voucher.IncrementUsedCount();
        var result = VoucherEligibilityEvaluator.EvaluateAvailability(voucher, scenario == "user-limit" ? 1 : 0, null, false, Now);
        result.Error.Code.Should().Be(errorCode); result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void ZeroShippingFee_StillChecksVoucherValidityAndMinimumOrder()
    {
        var voucher = Voucher(type: DiscountType.FreeShip).ToSnapshot();
        var (_, tooSmall) = VoucherEligibilityEvaluator.EvaluatePlatformShippingVoucher(0m, 99m, voucher, Now);
        tooSmall.IsApplied.Should().BeFalse(); tooSmall.ErrorCode.Should().Be(PricingErrors.MinOrderAmountNotMet);
        var (_, expired) = VoucherEligibilityEvaluator.EvaluatePlatformShippingVoucher(0m, 100m, voucher, Now.AddDays(1));
        expired.IsApplied.Should().BeFalse(); expired.ErrorCode.Should().Be(PricingErrors.VoucherExpired);
        var (discount, valid) = VoucherEligibilityEvaluator.EvaluatePlatformShippingVoucher(0m, 100m, voucher, Now);
        valid.IsApplied.Should().BeTrue(); discount.Should().Be(0m);
    }
}
