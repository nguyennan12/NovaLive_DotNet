using NovaLive.Domain.Common;

namespace NovaLive.Domain.Pricing;

internal static class PricingInputValidator
{
    public static Result Validate(PriceCalculationInput input)
    {
        if (input.Shops == null || input.Shops.Count == 0)
            return PricingErrors.EmptyShops;

        if (input.Shops.Select(s => s.ShopId).Distinct().Count() != input.Shops.Count)
            return PricingErrors.DuplicateShop;

        foreach (var shop in input.Shops)
        {
            if (shop.ShippingFee < 0)
                return PricingErrors.NegativeShippingFee;

            if (shop.ShippingFee % 1 != 0)
                return PricingErrors.NonIntegerAmount;

            if (shop.Items == null || shop.Items.Count == 0)
                return PricingErrors.InvalidQuantity;

            if (shop.Items.Select(i => i.SkuId).Distinct().Count() != shop.Items.Count)
                return PricingErrors.DuplicateShop;

            foreach (var item in shop.Items)
            {
                if (item.Quantity <= 0)
                    return PricingErrors.InvalidQuantity;

                if (item.UnitPrice < 0)
                    return PricingErrors.NegativePrice;

                if (item.UnitPrice % 1 != 0)
                    return PricingErrors.NonIntegerAmount;
            }

            if (shop.ShopVoucher != null)
            {
                var voucher = shop.ShopVoucher;
                if (voucher.ShopId != shop.ShopId)
                    return PricingErrors.ShopMismatch;

                if (voucher.DiscountType != DiscountType.PercentCart &&
                    voucher.DiscountType != DiscountType.FixedCart)
                    return PricingErrors.InvalidVoucherTypeForSlot;

                if (!voucher.HasIntegerAmounts())
                    return PricingErrors.NonIntegerAmount;
            }
        }

        if (input.PlatformProductVoucher != null)
        {
            var voucher = input.PlatformProductVoucher;
            if (voucher.ShopId != null)
                return PricingErrors.PlatformScopeMismatch;

            if (voucher.DiscountType != DiscountType.PercentCart &&
                voucher.DiscountType != DiscountType.FixedCart)
                return PricingErrors.InvalidVoucherTypeForSlot;

            if (!voucher.HasIntegerAmounts())
                return PricingErrors.NonIntegerAmount;
        }

        if (input.PlatformShippingVoucher != null)
        {
            var voucher = input.PlatformShippingVoucher;
            if (voucher.ShopId != null)
                return PricingErrors.PlatformScopeMismatch;

            if (voucher.DiscountType != DiscountType.FreeShip &&
                voucher.DiscountType != DiscountType.PercentShip)
                return PricingErrors.InvalidVoucherTypeForSlot;

            if (!voucher.HasIntegerAmounts())
                return PricingErrors.NonIntegerAmount;
        }

        return Result.Success();
    }
}
