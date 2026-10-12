using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Pricing;

namespace NovaLive.Application.UseCases.Discounts.Queries.ValidateVoucher;

public sealed class ValidateVoucherQueryHandler(
    IDiscountRepository discountRepository,
    ICartRepository cartRepository,
    ISkuRepository skuRepository,
    ISpuRepository spuRepository,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<ValidateVoucherQuery, VoucherValidationResponse>
{
    public async Task<Result<VoucherValidationResponse>> Handle(ValidateVoucherQuery query, CancellationToken ct)
    {
        var normalizedCode = query.Request.Code.Trim().ToUpperInvariant();

        // 1. Tra cứu Voucher theo Code qua IDiscountRepository
        var discount = await discountRepository.GetByCodeAsync(normalizedCode, ct);
        if (discount is null)
        {
            return new VoucherValidationResponse(false, query.Request.Code, 0m, "Mã voucher không tồn tại.");
        }

        // 2. Kiểm tra tổng lượt dùng toàn sàn (used_count < max_uses)
        if (discount.MaxUses.HasValue && discount.UsedCount >= discount.MaxUses.Value)
        {
            return new VoucherValidationResponse(false, discount.Code, 0m, "Mã voucher đã hết lượt sử dụng trên hệ thống.");
        }

        // 3. Kiểm tra số lượt user đã dùng trong DiscountUsages qua IDiscountRepository
        if (currentUser.UserId.HasValue)
        {
            var userUsageCount = await discountRepository.GetUserUsageCountAsync(
                discount.Id, currentUser.UserId.Value, ct);

            if (userUsageCount >= discount.PerUserLimit)
            {
                return new VoucherValidationResponse(false, discount.Code, 0m, "Bạn đã sử dụng hết lượt cho phép của mã voucher này.");
            }
        }

        // 4. Tải CartItems từ ICartRepository
        var cartItems = await cartRepository.GetCartItemsByIdsAsync(
            query.Request.CartItemIds,
            currentUser.UserId,
            ct);

        if (cartItems.Count == 0)
        {
            return new VoucherValidationResponse(false, discount.Code, 0m, "Không tìm thấy sản phẩm hợp lệ trong giỏ hàng.");
        }

        // 5. Tải Skus từ ISkuRepository (lấy giá bán SellPrice hiện hành theo T20)
        var skuIds = cartItems.Select(ci => ci.SkuId).Distinct().ToList();
        var skus = await skuRepository.GetByIdsAsync(skuIds, ct);
        var activeSkuMap = skus.Where(s => s.IsActive).ToDictionary(s => s.Id);

        // 6. Tải Spus từ ISpuRepository (lấy CategoryId để kiểm tra điều kiện áp dụng)
        var spuIds = activeSkuMap.Values.Select(s => s.SpuId).Distinct().ToList();
        var spus = await spuRepository.GetByIdsAsync(spuIds, ct);
        var spuMap = spus.ToDictionary(s => s.Id);

        // 7. Ghép dữ liệu thành OrderItemCalculationInput
        var validCartInputs = new List<(Guid ShopId, OrderItemCalculationInput Item)>();
        foreach (var item in cartItems)
        {
            if (activeSkuMap.TryGetValue(item.SkuId, out var sku) &&
                spuMap.TryGetValue(sku.SpuId, out var spu))
            {
                validCartInputs.Add((
                    item.ShopId,
                    new OrderItemCalculationInput(sku.Id, spu.Id, spu.CategoryId, item.Quantity, sku.SellPrice)));
            }
        }

        if (validCartInputs.Count == 0)
        {
            return new VoucherValidationResponse(false, discount.Code, 0m, "Không có sản phẩm nào hợp lệ còn hoạt động trong giỏ hàng.");
        }

        var now = dateTimeProvider.UtcNow;
        var snapshot = discount.ToSnapshot();

        // 8. Đánh giá tính hợp lệ bằng Domain VoucherEligibilityEvaluator (tránh nhân đôi logic)
        if (discount.ShopId.HasValue)
        {
            if (query.Request.ShopId.HasValue && query.Request.ShopId.Value != discount.ShopId.Value)
            {
                return new VoucherValidationResponse(false, discount.Code, 0m, "Voucher shop không thuộc gian hàng của sản phẩm đang chọn.");
            }

            var shopCartItems = validCartInputs
                .Where(x => x.ShopId == discount.ShopId.Value)
                .Select(x => x.Item)
                .ToList();

            if (shopCartItems.Count == 0)
            {
                return new VoucherValidationResponse(false, discount.Code, 0m, "Không có sản phẩm nào thuộc gian hàng phát hành voucher.");
            }

            var shopInput = new ShopCalculationInput(discount.ShopId.Value, 0m, shopCartItems, snapshot);
            var (_, evaluation) = VoucherEligibilityEvaluator.EvaluateShopVoucher(shopInput, now);

            return new VoucherValidationResponse(
                evaluation.IsApplied,
                discount.Code,
                evaluation.DiscountAmount,
                evaluation.ErrorMessage);
        }

        // Tầng 2: Platform Product Voucher
        if (discount.DiscountType is DiscountType.PercentCart or DiscountType.FixedCart)
        {
            var shopGroups = validCartInputs
                .GroupBy(x => x.ShopId)
                .Select(g => new ShopCalculationInput(
                    g.Key,
                    0m,
                    g.Select(x => x.Item).ToList(),
                    null))
                .ToList();

            var emptyShopDiscounts = validCartInputs.ToDictionary(x => x.Item.SkuId, _ => 0m);
            var (_, _, evaluation) = VoucherEligibilityEvaluator.EvaluatePlatformProductVoucher(
                shopGroups, emptyShopDiscounts, snapshot, now);

            return new VoucherValidationResponse(
                evaluation.IsApplied,
                discount.Code,
                evaluation.DiscountAmount,
                evaluation.ErrorMessage);
        }

        // Tầng 3: Platform FreeShip Voucher
        var (isValid, _, errorMessage) = snapshot.CheckValidity(now);
        if (!isValid)
        {
            return new VoucherValidationResponse(false, discount.Code, 0m, errorMessage);
        }

        decimal netOrderGoodsAmount = validCartInputs.Sum(x => x.Item.Quantity * x.Item.UnitPrice);
        if (!snapshot.MeetsMinOrderAmount(netOrderGoodsAmount))
        {
            return new VoucherValidationResponse(false, discount.Code, 0m, "Giá trị đơn hàng chưa đạt mức tối thiểu của voucher freeship.");
        }

        return new VoucherValidationResponse(true, discount.Code, 0m, null);
    }
}
