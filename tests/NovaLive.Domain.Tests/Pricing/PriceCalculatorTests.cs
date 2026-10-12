using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Pricing;
using Xunit;

namespace NovaLive.Domain.Tests.Pricing;

public sealed class PriceCalculatorTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 10, 12, 10, 0, 0, TimeSpan.Zero);

    // =========================================================================
    // INVARIANT HELPER (Dùng kiểm tra mọi test hợp lệ)
    // =========================================================================
    private static void AssertAccountingInvariants(PriceCalculationResult result)
    {
        // Invariant 1: Đẳng thức Grand Total
        decimal expectedGrandTotal = result.TotalItemAmount + result.TotalShippingFee
            - result.TotalShopDiscount - result.TotalPlatformDiscount - result.TotalShippingDiscount;
        result.GrandTotal.Should().Be(expectedGrandTotal);
        result.ShopResults.Sum(s => s.ShopTotal).Should().Be(result.GrandTotal);

        // Invariant 2: Không âm tiền
        result.GrandTotal.Should().BeGreaterOrEqualTo(0);
        foreach (var s in result.ShopResults)
        {
            s.ShopTotal.Should().BeGreaterOrEqualTo(0);
            s.NetGoodsAmount.Should().BeGreaterOrEqualTo(0);
            s.ShippingFee.Should().BeGreaterOrEqualTo(s.ShippingDiscountAmount);
            s.ItemsSubtotal.Should().BeGreaterOrEqualTo(s.ShopDiscountAmount + s.PlatformDiscountAmount);

            // Invariant 3: Khớp nối cấp Item và cấp Shop
            s.Items.Sum(i => i.ShopDiscountAmount).Should().Be(s.ShopDiscountAmount);
            s.Items.Sum(i => i.PlatformDiscountAmount).Should().Be(s.PlatformDiscountAmount);
            s.Items.Sum(i => i.TotalDiscountAmount).Should().Be(s.ShopDiscountAmount + s.PlatformDiscountAmount);
            s.Items.Sum(i => i.FinalLineTotal).Should().Be(s.NetGoodsAmount);
        }
    }

    // =========================================================================
    // TEST CASES THEO ĐÚNG KẾ HOẠCH
    // =========================================================================

    [Fact]
    public void Calculate_WithThreeShops_ShouldDistributePennyRemainderDeterministicallyByShopId()
    {
        // Sắp xếp các GUID theo thứ tự xác định: guid1 < guid2 < guid3
        var guids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
            .OrderBy(g => g)
            .ToArray();

        var shopA = guids[0]; // Nhỏ nhất -> sẽ nhận 1đ dư do tie-break Guid.CompareTo
        var shopB = guids[1];
        var shopC = guids[2];

        var shops = new List<ShopCalculationInput>
        {
            new(shopA, 15000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 100000m) }, null),
            new(shopB, 15000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 100000m) }, null),
            new(shopC, 15000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 100000m) }, null)
        };

        var platformVoucher = new VoucherSnapshot(
            Guid.NewGuid(),
            null,
            "PLATFORM10K",
            DiscountType.FixedCart,
            10000m,
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var input = new PriceCalculationInput(shops, platformVoucher, null, FixedNow);

        var result = PriceCalculator.Calculate(input);

        result.IsSuccess.Should().BeTrue();
        var val = result.Value!;
        AssertAccountingInvariants(val);

        val.TotalPlatformDiscount.Should().Be(10000m);

        var resA = val.ShopResults.First(s => s.ShopId == shopA);
        var resB = val.ShopResults.First(s => s.ShopId == shopB);
        var resC = val.ShopResults.First(s => s.ShopId == shopC);

        // 10000 / 3 = 3333 dư 1 -> Shop A có ID nhỏ nhất nhận phần dư 1đ
        resA.PlatformDiscountAmount.Should().Be(3334m);
        resB.PlatformDiscountAmount.Should().Be(3333m);
        resC.PlatformDiscountAmount.Should().Be(3333m);
    }

    [Fact]
    public void Calculate_WhenOneShopHitsCap_ShouldNotGivePennyToCappedShop()
    {
        var shop1Id = Guid.NewGuid();
        var shop2Id = Guid.NewGuid();

        // Shop 1: Subtotal 100k, Shop voucher 100k -> Cap còn lại = 0đ
        var shop1Voucher = new VoucherSnapshot(
            Guid.NewGuid(),
            shop1Id,
            "SHOP1_100K",
            DiscountType.FixedCart,
            100000m,
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var shops = new List<ShopCalculationInput>
        {
            new(shop1Id, 10000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 100000m) }, shop1Voucher),
            new(shop2Id, 10000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 200000m) }, null)
        };

        var platformVoucher = new VoucherSnapshot(
            Guid.NewGuid(),
            null,
            "PLATFORM_50K",
            DiscountType.FixedCart,
            50000m,
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var input = new PriceCalculationInput(shops, platformVoucher, null, FixedNow);

        var result = PriceCalculator.Calculate(input);

        result.IsSuccess.Should().BeTrue();
        var val = result.Value!;
        AssertAccountingInvariants(val);

        var res1 = val.ShopResults.First(s => s.ShopId == shop1Id);
        var res2 = val.ShopResults.First(s => s.ShopId == shop2Id);

        res1.ShopDiscountAmount.Should().Be(100000m);
        res1.PlatformDiscountAmount.Should().Be(0m);
        res1.NetGoodsAmount.Should().Be(0m);

        res2.PlatformDiscountAmount.Should().Be(50000m);
        res2.NetGoodsAmount.Should().Be(150000m);
    }

    [Fact]
    public void Calculate_WhenTotalShippingFeeIsZero_ShouldNotDivideByZero()
    {
        var shopId = Guid.NewGuid();
        var shops = new List<ShopCalculationInput>
        {
            new(shopId, 0m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 50000m) }, null)
        };

        var freeshipVoucher = new VoucherSnapshot(
            Guid.NewGuid(),
            null,
            "FREESHIP30K",
            DiscountType.FreeShip,
            30000m,
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var input = new PriceCalculationInput(shops, null, freeshipVoucher, FixedNow);

        var result = PriceCalculator.Calculate(input);

        result.IsSuccess.Should().BeTrue();
        var val = result.Value!;
        AssertAccountingInvariants(val);

        val.TotalShippingFee.Should().Be(0m);
        val.TotalShippingDiscount.Should().Be(0m);
    }

    [Fact]
    public void Calculate_WhenShopAndPlatformVouchersHaveDifferentEligibleSpus_ShouldProrateIndependently()
    {
        var shopId = Guid.NewGuid();
        var spu1 = Guid.NewGuid();
        var spu2 = Guid.NewGuid();
        var spu3 = Guid.NewGuid();

        var item1 = new OrderItemCalculationInput(Guid.NewGuid(), spu1, null, 1, 100000m);
        var item2 = new OrderItemCalculationInput(Guid.NewGuid(), spu2, null, 1, 100000m);
        var item3 = new OrderItemCalculationInput(Guid.NewGuid(), spu3, null, 1, 100000m);

        // Shop voucher chỉ áp dụng cho SPU 1
        var shopVoucher = new VoucherSnapshot(
            Guid.NewGuid(),
            shopId,
            "SHOP_SPU1",
            DiscountType.FixedCart,
            20000m,
            0m,
            null,
            DiscountAppliesTo.SpecificSpus,
            new[] { spu1 },
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        // Platform voucher chỉ áp dụng cho SPU 2
        var platformVoucher = new VoucherSnapshot(
            Guid.NewGuid(),
            null,
            "PLATFORM_SPU2",
            DiscountType.FixedCart,
            30000m,
            0m,
            null,
            DiscountAppliesTo.SpecificSpus,
            new[] { spu2 },
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var shops = new List<ShopCalculationInput>
        {
            new(shopId, 15000m, new[] { item1, item2, item3 }, shopVoucher)
        };

        var input = new PriceCalculationInput(shops, platformVoucher, null, FixedNow);

        var result = PriceCalculator.Calculate(input);

        result.IsSuccess.Should().BeTrue();
        var val = result.Value!;
        AssertAccountingInvariants(val);

        var shopRes = val.ShopResults.First();
        var resItem1 = shopRes.Items.First(i => i.SkuId == item1.SkuId);
        var resItem2 = shopRes.Items.First(i => i.SkuId == item2.SkuId);
        var resItem3 = shopRes.Items.First(i => i.SkuId == item3.SkuId);

        // Item 1 chỉ nhận shop discount
        resItem1.ShopDiscountAmount.Should().Be(20000m);
        resItem1.PlatformDiscountAmount.Should().Be(0m);
        resItem1.FinalLineTotal.Should().Be(80000m);

        // Item 2 chỉ nhận platform discount
        resItem2.ShopDiscountAmount.Should().Be(0m);
        resItem2.PlatformDiscountAmount.Should().Be(30000m);
        resItem2.FinalLineTotal.Should().Be(70000m);

        // Item 3 không nhận discount nào
        resItem3.ShopDiscountAmount.Should().Be(0m);
        resItem3.PlatformDiscountAmount.Should().Be(0m);
        resItem3.FinalLineTotal.Should().Be(100000m);
    }

    [Fact]
    public void Calculate_WhenVoucherHasSlotMismatch_ShouldReturnFailure()
    {
        var shopA = Guid.NewGuid();
        var shopB = Guid.NewGuid();

        // Voucher của Shop A nhưng gắn vào slot của Shop B
        var voucherShopA = new VoucherSnapshot(
            Guid.NewGuid(),
            shopA,
            "SHOPA",
            DiscountType.FixedCart,
            10000m,
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var shops = new List<ShopCalculationInput>
        {
            new(shopB, 10000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 50000m) }, voucherShopA)
        };

        var input = new PriceCalculationInput(shops, null, null, FixedNow);

        var result = PriceCalculator.Calculate(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(PricingErrors.ShopMismatch.Code);
    }

    [Fact]
    public void Calculate_WhenPlatformVoucherHasShopId_ShouldReturnFailure()
    {
        var shopId = Guid.NewGuid();
        var shops = new List<ShopCalculationInput>
        {
            new(shopId, 10000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 50000m) }, null)
        };

        // Platform voucher nhưng lại có ShopId != null
        var badPlatformVoucher = new VoucherSnapshot(
            Guid.NewGuid(),
            shopId,
            "BAD_PLATFORM",
            DiscountType.FixedCart,
            10000m,
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var input = new PriceCalculationInput(shops, badPlatformVoucher, null, FixedNow);

        var result = PriceCalculator.Calculate(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(PricingErrors.PlatformScopeMismatch.Code);
    }

    [Fact]
    public void Calculate_WhenSlotReceivesInvalidVoucherType_ShouldReturnFailure()
    {
        var shopId = Guid.NewGuid();

        // Shop slot nhận FreeShip -> Sai quy định slot
        var freeShipInShopSlot = new VoucherSnapshot(
            Guid.NewGuid(),
            shopId,
            "FREESHIP",
            DiscountType.FreeShip,
            10000m,
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var shops = new List<ShopCalculationInput>
        {
            new(shopId, 10000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 50000m) }, freeShipInShopSlot)
        };

        var input = new PriceCalculationInput(shops, null, null, FixedNow);

        var result = PriceCalculator.Calculate(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(PricingErrors.InvalidVoucherTypeForSlot.Code);
    }

    [Fact]
    public void Calculate_WhenInputHasNonIntegerVndAmount_ShouldReturnFailure()
    {
        var shopId = Guid.NewGuid();
        // Đơn giá lẻ 10000.5đ
        var shops = new List<ShopCalculationInput>
        {
            new(shopId, 10000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 10000.5m) }, null)
        };

        var input = new PriceCalculationInput(shops, null, null, FixedNow);

        var result = PriceCalculator.Calculate(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(PricingErrors.NonIntegerAmount.Code);
    }

    [Fact]
    public void Calculate_WhenCurrentTimeIsExactBoundary_ShouldBeApplied()
    {
        var shopId = Guid.NewGuid();
        var validFrom = FixedNow;
        var validTo = FixedNow.AddHours(2);

        var voucher = new VoucherSnapshot(
            Guid.NewGuid(),
            shopId,
            "EXACT_TIME",
            DiscountType.FixedCart,
            10000m,
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            validFrom,
            validTo,
            true);

        var shops = new List<ShopCalculationInput>
        {
            new(shopId, 10000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 50000m) }, voucher)
        };

        // 1. Biên bắt đầu: CurrentTime == ValidFrom -> Thành công
        var res1 = PriceCalculator.Calculate(new PriceCalculationInput(shops, null, null, validFrom));
        res1.IsSuccess.Should().BeTrue();
        res1.Value!.ShopResults.First().ShopDiscountAmount.Should().Be(10000m);

        // 2. Biên kết thúc: CurrentTime == ValidTo -> Thành công
        var res2 = PriceCalculator.Calculate(new PriceCalculationInput(shops, null, null, validTo));
        res2.IsSuccess.Should().BeTrue();
        res2.Value!.ShopResults.First().ShopDiscountAmount.Should().Be(10000m);

        // 3. Quá 1 giây: CurrentTime == ValidTo + 1s -> Hết hạn (Rejected)
        var res3 = PriceCalculator.Calculate(new PriceCalculationInput(shops, null, null, validTo.AddSeconds(1)));
        res3.IsSuccess.Should().BeTrue();
        res3.Value!.ShopResults.First().ShopDiscountAmount.Should().Be(0m);
        res3.Value.VoucherEvaluations.First().ErrorCode.Should().Be(PricingErrors.VoucherExpired);

        // 4. Trước 1 giây: CurrentTime == ValidFrom - 1s -> Chưa đến hạn (Rejected)
        var res4 = PriceCalculator.Calculate(new PriceCalculationInput(shops, null, null, validFrom.AddSeconds(-1)));
        res4.IsSuccess.Should().BeTrue();
        res4.Value!.ShopResults.First().ShopDiscountAmount.Should().Be(0m);
        res4.Value.VoucherEvaluations.First().ErrorCode.Should().Be(PricingErrors.VoucherNotYetActive);
    }

    [Fact]
    public void Calculate_WhenSubtotalIsExactBoundaryOfMinOrderAmount()
    {
        var shopId = Guid.NewGuid();

        var voucher = new VoucherSnapshot(
            Guid.NewGuid(),
            shopId,
            "MIN_100K",
            DiscountType.FixedCart,
            10000m,
            100000m, // Ngưỡng 100k
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        // 1. Subtotal == 100,000đ -> Áp dụng thành công
        var shopsExact = new List<ShopCalculationInput>
        {
            new(shopId, 10000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 100000m) }, voucher)
        };
        var res1 = PriceCalculator.Calculate(new PriceCalculationInput(shopsExact, null, null, FixedNow));
        res1.IsSuccess.Should().BeTrue();
        res1.Value!.ShopResults.First().ShopDiscountAmount.Should().Be(10000m);

        // 2. Subtotal == 99,999đ -> Bị từ chối (MinOrderAmountNotMet)
        var shopsUnder = new List<ShopCalculationInput>
        {
            new(shopId, 10000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 99999m) }, voucher)
        };
        var res2 = PriceCalculator.Calculate(new PriceCalculationInput(shopsUnder, null, null, FixedNow));
        res2.IsSuccess.Should().BeTrue();
        res2.Value!.ShopResults.First().ShopDiscountAmount.Should().Be(0m);
        res2.Value.VoucherEvaluations.First().ErrorCode.Should().Be(PricingErrors.MinOrderAmountNotMet);
    }

    [Fact]
    public void Calculate_ShopOrderInvariance_ShouldYieldIdenticalResultsRegardlessOfInputOrder()
    {
        var shopA = Guid.NewGuid();
        var shopB = Guid.NewGuid();
        var shopC = Guid.NewGuid();

        var inputA = new ShopCalculationInput(shopA, 10000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 150000m) }, null);
        var inputB = new ShopCalculationInput(shopB, 20000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 250000m) }, null);
        var inputC = new ShopCalculationInput(shopC, 30000m, new[] { new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 350000m) }, null);

        var platformVoucher = new VoucherSnapshot(
            Guid.NewGuid(),
            null,
            "PLATFORM_15PCT",
            DiscountType.PercentCart,
            15m, // 15%
            0m,
            50000m, // Capped 50k
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var freeshipVoucher = new VoucherSnapshot(
            Guid.NewGuid(),
            null,
            "FREESHIP_40K",
            DiscountType.FreeShip,
            40000m,
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        // Thứ tự 1: [A, B, C]
        var list1 = new List<ShopCalculationInput> { inputA, inputB, inputC };
        var res1 = PriceCalculator.Calculate(new PriceCalculationInput(list1, platformVoucher, freeshipVoucher, FixedNow));

        // Thứ tự 2: [C, A, B]
        var list2 = new List<ShopCalculationInput> { inputC, inputA, inputB };
        var res2 = PriceCalculator.Calculate(new PriceCalculationInput(list2, platformVoucher, freeshipVoucher, FixedNow));

        res1.IsSuccess.Should().BeTrue();
        res2.IsSuccess.Should().BeTrue();

        var val1 = res1.Value!;
        var val2 = res2.Value!;

        val1.GrandTotal.Should().Be(val2.GrandTotal);
        val1.TotalPlatformDiscount.Should().Be(val2.TotalPlatformDiscount);
        val1.TotalShippingDiscount.Should().Be(val2.TotalShippingDiscount);

        foreach (var s1 in val1.ShopResults)
        {
            var s2 = val2.ShopResults.First(x => x.ShopId == s1.ShopId);
            s1.PlatformDiscountAmount.Should().Be(s2.PlatformDiscountAmount);
            s1.ShippingDiscountAmount.Should().Be(s2.ShippingDiscountAmount);
            s1.ShopTotal.Should().Be(s2.ShopTotal);
        }
    }

    [Fact]
    public void Calculate_WithFullThreeTierCombination_ShouldProrateAccurately()
    {
        var shop1Id = Guid.NewGuid();
        var shop2Id = Guid.NewGuid();

        var item1 = new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 2, 100000m); // 200k
        var item2 = new OrderItemCalculationInput(Guid.NewGuid(), Guid.NewGuid(), null, 1, 300000m); // 300k

        var shop1Voucher = new VoucherSnapshot(
            Guid.NewGuid(),
            shop1Id,
            "SHOP1_20K",
            DiscountType.FixedCart,
            20000m,
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var platformProductVoucher = new VoucherSnapshot(
            Guid.NewGuid(),
            null,
            "PLATFORM_10PCT",
            DiscountType.PercentCart,
            10m, // 10%
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var platformFreeshipVoucher = new VoucherSnapshot(
            Guid.NewGuid(),
            null,
            "FREESHIP_30K",
            DiscountType.FreeShip,
            30000m,
            0m,
            null,
            DiscountAppliesTo.AllProducts,
            null,
            FixedNow.AddDays(-1),
            FixedNow.AddDays(1),
            true);

        var shops = new List<ShopCalculationInput>
        {
            new(shop1Id, 25000m, new[] { item1 }, shop1Voucher),
            new(shop2Id, 35000m, new[] { item2 }, null)
        };

        var input = new PriceCalculationInput(shops, platformProductVoucher, platformFreeshipVoucher, FixedNow);

        var result = PriceCalculator.Calculate(input);

        result.IsSuccess.Should().BeTrue();
        var val = result.Value!;
        AssertAccountingInvariants(val);

        // Shop 1: Subtotal = 200k. Shop discount = 20k -> EligibleAfterShop = 180k.
        // Shop 2: Subtotal = 300k. Shop discount = 0k -> EligibleAfterShop = 300k.
        // TotalEligibleAfterShop = 480k.
        // Platform Voucher 10% = 48k.
        // Shop 1 share = 48k * 180 / 480 = 18,000đ.
        // Shop 2 share = 48k * 300 / 480 = 30,000đ.
        val.ShopResults.First(s => s.ShopId == shop1Id).PlatformDiscountAmount.Should().Be(18000m);
        val.ShopResults.First(s => s.ShopId == shop2Id).PlatformDiscountAmount.Should().Be(30000m);

        // FreeShip 30k trên tổng phí ship 60k (25k + 35k)
        // Shop 1 shipping share = 30k * 25 / 60 = 12,500đ.
        // Shop 2 shipping share = 30k * 35 / 60 = 17,500đ.
        val.TotalShippingDiscount.Should().Be(30000m);
    }

    [Fact]
    public void VoucherSnapshot_CalculateDiscount_ShouldCalculateCorrectlyBasedOnTypeAndCap()
    {
        // 1. Fixed cart: 50,000đ trên đơn 40,000đ -> chỉ giảm 40,000đ (không giảm quá base)
        var fixedVoucher = new VoucherSnapshot(
            Guid.NewGuid(), null, "FIX50", DiscountType.FixedCart, 50000m, 0m, null,
            DiscountAppliesTo.AllProducts, null, FixedNow.AddDays(-1), FixedNow.AddDays(1), true);

        fixedVoucher.CalculateDiscount(40000m).Should().Be(40000m);
        fixedVoucher.CalculateDiscount(100000m).Should().Be(50000m);
        fixedVoucher.CalculateDiscount(0m).Should().Be(0m);

        // 2. Percent cart: 10% trần 25,000đ
        var percentVoucher = new VoucherSnapshot(
            Guid.NewGuid(), null, "PCT10", DiscountType.PercentCart, 10m, 0m, 25000m,
            DiscountAppliesTo.AllProducts, null, FixedNow.AddDays(-1), FixedNow.AddDays(1), true);

        percentVoucher.CalculateDiscount(100000m).Should().Be(10000m); // 10% của 100k = 10k (< trần 25k)
        percentVoucher.CalculateDiscount(300000m).Should().Be(25000m); // 10% của 300k = 30k (chạm trần 25k)
    }
}
