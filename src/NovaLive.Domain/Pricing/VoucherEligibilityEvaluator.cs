using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;

namespace NovaLive.Domain.Pricing;

public static class VoucherEligibilityEvaluator
{
    public static (decimal Discount, VoucherEvaluationResult Evaluation) EvaluateShopVoucher(
        ShopCalculationInput shop,
        DateTimeOffset currentTime)
    {
        var voucher = shop.ShopVoucher!;

        var (isValid, errorCode, errorMessage) = voucher.CheckValidity(currentTime);
        if (!isValid)
        {
            return (0m, new VoucherEvaluationResult(
                voucher.Code, voucher.DiscountType, shop.ShopId, false, 0m, errorCode, errorMessage));
        }

        var eligibleItems = shop.Items.Where(i => voucher.IsItemEligible(i.SpuId, i.CategoryId)).ToList();
        decimal eligibleSubtotal = eligibleItems.Sum(i => i.UnitPrice * i.Quantity);

        if (eligibleItems.Count == 0 || eligibleSubtotal <= 0)
        {
            return (0m, new VoucherEvaluationResult(
                voucher.Code, voucher.DiscountType, shop.ShopId, false, 0m,
                PricingErrors.NoEligibleItems, "Không có sản phẩm nào thỏa mãn điều kiện áp dụng voucher shop."));
        }

        if (!voucher.MeetsMinOrderAmount(eligibleSubtotal))
        {
            return (0m, new VoucherEvaluationResult(
                voucher.Code, voucher.DiscountType, shop.ShopId, false, 0m,
                PricingErrors.MinOrderAmountNotMet, "Giá trị sản phẩm áp dụng chưa đạt mức tối thiểu của voucher shop."));
        }

        decimal discount = voucher.CalculateDiscount(eligibleSubtotal);

        return (discount, new VoucherEvaluationResult(
            voucher.Code, voucher.DiscountType, shop.ShopId, true, discount, null, null));
    }

    public static (decimal BudgetV, Dictionary<Guid, decimal> ShopEligibleAfterShop, VoucherEvaluationResult Evaluation) EvaluatePlatformProductVoucher(
        IReadOnlyList<ShopCalculationInput> shops,
        IReadOnlyDictionary<Guid, decimal> itemShopDiscounts,
        VoucherSnapshot voucher,
        DateTimeOffset currentTime)
    {
        var emptyMap = shops.ToDictionary(s => s.ShopId, _ => 0m);

        var (isValid, errorCode, errorMessage) = voucher.CheckValidity(currentTime);
        if (!isValid)
        {
            return (0m, emptyMap, new VoucherEvaluationResult(
                voucher.Code, voucher.DiscountType, null, false, 0m, errorCode, errorMessage));
        }

        var shopEligibleAfterShop = new Dictionary<Guid, decimal>();
        decimal totalEligibleAfterShop = 0m;

        foreach (var shop in shops)
        {
            decimal shopEligible = 0m;
            foreach (var item in shop.Items)
            {
                if (voucher.IsItemEligible(item.SpuId, item.CategoryId))
                {
                    decimal lineTotal = item.UnitPrice * item.Quantity;
                    decimal afterShop = lineTotal - itemShopDiscounts[item.SkuId];
                    if (afterShop > 0)
                        shopEligible += afterShop;
                }
            }
            shopEligibleAfterShop[shop.ShopId] = shopEligible;
            totalEligibleAfterShop += shopEligible;
        }

        if (totalEligibleAfterShop <= 0)
        {
            return (0m, emptyMap, new VoucherEvaluationResult(
                voucher.Code, voucher.DiscountType, null, false, 0m,
                PricingErrors.NoEligibleItems, "Không có sản phẩm nào thỏa mãn điều kiện áp dụng voucher sàn."));
        }

        if (!voucher.MeetsMinOrderAmount(totalEligibleAfterShop))
        {
            return (0m, emptyMap, new VoucherEvaluationResult(
                voucher.Code, voucher.DiscountType, null, false, 0m,
                PricingErrors.MinOrderAmountNotMet, "Tổng giá trị hàng sau giảm giá shop chưa đạt mức tối thiểu của voucher sàn."));
        }

        decimal budgetV = voucher.CalculateDiscount(totalEligibleAfterShop);

        return (budgetV, shopEligibleAfterShop, new VoucherEvaluationResult(
            voucher.Code, voucher.DiscountType, null, true, budgetV, null, null));
    }

    public static (decimal BudgetF, VoucherEvaluationResult Evaluation) EvaluatePlatformShippingVoucher(
        decimal totalShippingFee,
        decimal netOrderGoodsAmount,
        VoucherSnapshot voucher,
        DateTimeOffset currentTime)
    {
        if (totalShippingFee == 0)
        {
            return (0m, new VoucherEvaluationResult(
                voucher.Code, voucher.DiscountType, null, true, 0m, null, null));
        }

        var (isValid, errorCode, errorMessage) = voucher.CheckValidity(currentTime);
        if (!isValid)
        {
            return (0m, new VoucherEvaluationResult(
                voucher.Code, voucher.DiscountType, null, false, 0m, errorCode, errorMessage));
        }

        if (!voucher.MeetsMinOrderAmount(netOrderGoodsAmount))
        {
            return (0m, new VoucherEvaluationResult(
                voucher.Code, voucher.DiscountType, null, false, 0m,
                PricingErrors.MinOrderAmountNotMet, "Giá trị đơn hàng thực trả chưa đạt mức tối thiểu của voucher freeship."));
        }

        decimal budgetF = voucher.CalculateDiscount(totalShippingFee);

        return (budgetF, new VoucherEvaluationResult(
            voucher.Code, voucher.DiscountType, null, true, budgetF, null, null));
    }
}
