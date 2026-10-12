using NovaLive.Domain.Common;

namespace NovaLive.Domain.Pricing;

public static class PriceCalculator
{
    public static Result<PriceCalculationResult> Calculate(PriceCalculationInput input)
    {
        // -------------------------------------------------------------------------
        // Bước 0: Input & Slot Validation
        // -------------------------------------------------------------------------
        var validationResult = PricingInputValidator.Validate(input);
        if (validationResult.IsFailure)
            return validationResult.Error;

        var voucherEvaluations = new List<VoucherEvaluationResult>();

        // -------------------------------------------------------------------------
        // Bước 1: Tính Shop Subtotal & Cước Phí
        // -------------------------------------------------------------------------
        decimal totalItemAmount = 0m;
        decimal totalShippingFee = 0m;
        var shopSubtotals = new Dictionary<Guid, decimal>();

        foreach (var shop in input.Shops)
        {
            decimal subtotal = shop.Items.Sum(i => i.UnitPrice * i.Quantity);
            shopSubtotals[shop.ShopId] = subtotal;
            totalItemAmount += subtotal;
            totalShippingFee += shop.ShippingFee;
        }

        // -------------------------------------------------------------------------
        // Bước 2: Tầng 1 — Voucher Shop & Phân Bổ Item-Level
        // -------------------------------------------------------------------------
        var shopDiscounts = new Dictionary<Guid, decimal>();
        var itemShopDiscounts = new Dictionary<Guid, decimal>();

        foreach (var shop in input.Shops)
        {
            foreach (var item in shop.Items)
                itemShopDiscounts[item.SkuId] = 0m;

            decimal shopDiscount = 0m;
            if (shop.ShopVoucher != null)
            {
                var (evalDiscount, evaluation) = VoucherEligibilityEvaluator.EvaluateShopVoucher(shop, input.CurrentTime);
                voucherEvaluations.Add(evaluation);
                shopDiscount = evalDiscount;

                if (shopDiscount > 0)
                {
                    var eligibleItems = shop.Items
                        .Where(i => shop.ShopVoucher.IsItemEligible(i.SpuId, i.CategoryId))
                        .ToList();

                    var itemCandidates = eligibleItems.Select(i =>
                    {
                        decimal lineTotal = i.UnitPrice * i.Quantity;
                        return new ProrationEngine.AllocationCandidate(i.SkuId, lineTotal, lineTotal);
                    }).ToList();

                    var allocated = ProrationEngine.Allocate(shopDiscount, itemCandidates);
                    foreach (var (skuId, disc) in allocated)
                        itemShopDiscounts[skuId] = disc;
                }
            }

            shopDiscounts[shop.ShopId] = shopDiscount;
        }

        // -------------------------------------------------------------------------
        // Bước 3: Tầng 2 — Platform Product Voucher Proration
        // -------------------------------------------------------------------------
        var platformDiscounts = input.Shops.ToDictionary(s => s.ShopId, _ => 0m);
        var itemPlatformDiscounts = input.Shops.SelectMany(s => s.Items).ToDictionary(i => i.SkuId, _ => 0m);

        if (input.PlatformProductVoucher != null)
        {
            var (budgetV, shopEligibleMap, evaluation) = VoucherEligibilityEvaluator.EvaluatePlatformProductVoucher(
                input.Shops, itemShopDiscounts, input.PlatformProductVoucher, input.CurrentTime);
            voucherEvaluations.Add(evaluation);

            if (budgetV > 0)
            {
                var shopCandidates = shopEligibleMap
                    .Select(kv => new ProrationEngine.AllocationCandidate(kv.Key, kv.Value, kv.Value))
                    .ToList();
                platformDiscounts = ProrationEngine.Allocate(budgetV, shopCandidates);
            }
        }

        // -------------------------------------------------------------------------
        // Bước 4: Tầng 3 — Platform FreeShip Voucher
        // -------------------------------------------------------------------------
        var shippingDiscounts = input.Shops.ToDictionary(s => s.ShopId, _ => 0m);

        if (input.PlatformShippingVoucher != null)
        {
            decimal totalShopDisc = shopDiscounts.Values.Sum();
            decimal totalPlatformDisc = platformDiscounts.Values.Sum();
            decimal netOrderGoodsAmount = totalItemAmount - totalShopDisc - totalPlatformDisc;

            var (budgetF, evaluation) = VoucherEligibilityEvaluator.EvaluatePlatformShippingVoucher(
                totalShippingFee, netOrderGoodsAmount, input.PlatformShippingVoucher, input.CurrentTime);
            voucherEvaluations.Add(evaluation);

            if (budgetF > 0)
            {
                var shippingCandidates = input.Shops
                    .Select(s => new ProrationEngine.AllocationCandidate(s.ShopId, s.ShippingFee, s.ShippingFee))
                    .ToList();
                shippingDiscounts = ProrationEngine.Allocate(budgetF, shippingCandidates);
            }
        }

        // -------------------------------------------------------------------------
        // Bước 5: Phân Bổ Item-Level Platform Discount & Tổng Kết
        // -------------------------------------------------------------------------
        foreach (var shop in input.Shops)
        {
            decimal shopPlatformDisc = platformDiscounts[shop.ShopId];
            if (shopPlatformDisc > 0 && input.PlatformProductVoucher != null)
            {
                var platformEligibleItems = shop.Items
                    .Where(i => input.PlatformProductVoucher.IsItemEligible(i.SpuId, i.CategoryId))
                    .Select(i =>
                    {
                        decimal lineTotal = i.UnitPrice * i.Quantity;
                        decimal cap = lineTotal - itemShopDiscounts[i.SkuId];
                        return new ProrationEngine.AllocationCandidate(i.SkuId, cap, cap);
                    })
                    .Where(c => c.Cap > 0)
                    .ToList();

                var allocated = ProrationEngine.Allocate(shopPlatformDisc, platformEligibleItems);
                foreach (var (skuId, disc) in allocated)
                    itemPlatformDiscounts[skuId] = disc;
            }
        }

        var shopResults = new List<ShopCalculationResult>();
        foreach (var shop in input.Shops)
        {
            decimal itemsSubtotal = shopSubtotals[shop.ShopId];
            decimal shopDisc = shopDiscounts[shop.ShopId];
            decimal platformDisc = platformDiscounts[shop.ShopId];
            decimal shippingDisc = shippingDiscounts[shop.ShopId];
            decimal netGoods = itemsSubtotal - shopDisc - platformDisc;
            decimal shopTotal = netGoods + (shop.ShippingFee - shippingDisc);

            var itemResults = shop.Items.Select(item =>
            {
                decimal lineTotal = item.UnitPrice * item.Quantity;
                decimal iShopDisc = itemShopDiscounts[item.SkuId];
                decimal iPlatformDisc = itemPlatformDiscounts[item.SkuId];
                decimal totalItemDisc = iShopDisc + iPlatformDisc;
                decimal finalLineTotal = lineTotal - totalItemDisc;

                return new OrderItemCalculationResult(
                    item.SkuId, item.SpuId, item.Quantity, item.UnitPrice,
                    lineTotal, iShopDisc, iPlatformDisc, totalItemDisc, finalLineTotal);
            }).ToList();

            shopResults.Add(new ShopCalculationResult(
                shop.ShopId, itemsSubtotal, shop.ShippingFee, shopDisc, platformDisc, shippingDisc,
                netGoods, shopTotal, itemResults));
        }

        decimal grandTotal = shopResults.Sum(s => s.ShopTotal);
        decimal totalShopDiscount = shopDiscounts.Values.Sum();
        decimal totalPlatformDiscount = platformDiscounts.Values.Sum();
        decimal totalShippingDiscount = shippingDiscounts.Values.Sum();

        decimal expectedGrandTotal = totalItemAmount + totalShippingFee
            - totalShopDiscount - totalPlatformDiscount - totalShippingDiscount;

        if (grandTotal != expectedGrandTotal)
            return PricingErrors.InternalAccountingMismatch;

        return new PriceCalculationResult(
            totalItemAmount, totalShippingFee, totalShopDiscount, totalPlatformDiscount, totalShippingDiscount,
            grandTotal, shopResults, voucherEvaluations);
    }
}
