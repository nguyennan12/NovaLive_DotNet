using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Pricing;
using NovaLive.Application.UseCases.FlashSales;

namespace NovaLive.Application.UseCases.Orders;

public sealed class CheckoutDraftBuilder(
    ICartRepository carts,
    ISkuRepository skus,
    ISpuRepository spus,
    IShopRepository shops,
    IInventoryRepository inventories,
    IUserAddressRepository addresses,
    IShopAddressRepository warehouses,
    IDiscountRepository discounts,
    IShippingProvider shipping,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    IFlashSaleRepository flashSales)

{
    public async Task<Result<PreparedCheckout>> BuildAsync(CalculateCheckoutRequest request, CancellationToken ct, IReadOnlyList<ShopShippingSelectionDto>? selections = null)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return CheckoutErrors.Unauthorized;

        if (request.CartItemIds.Count == 0 || request.CartItemIds.Distinct().Count() != request.CartItemIds.Count)
            return OrderErrors.EmptyCart;
        var cart = await carts.GetByUserIdAsync(userId, ct);
        if (cart is null || cart.UserId != userId)
            return CheckoutErrors.CartItemsNotFound;
        var items = await carts.GetCartItemsByIdsAsync(request.CartItemIds, userId, ct);
        if (items.Count != request.CartItemIds.Count || items.Any(i => i.CartId != cart.Id)
            || !items.Select(i => i.Id).ToHashSet().SetEquals(request.CartItemIds))
            return CheckoutErrors.CartItemsNotFound;

        var address = await addresses.GetByIdForUserAsync(request.ShippingAddressId, userId, ct);
        if (address is null || address.UserId != userId || address.Id != request.ShippingAddressId)
            return CheckoutErrors.AddressNotFound;
        if (address.DistrictId is not > 0 || string.IsNullOrWhiteSpace(address.WardCode))
            return CheckoutErrors.IncompleteAddress;

        // These repositories share the scoped EF context: never parallelize database calls.
        var skuIds = items.Select(i => i.SkuId).Distinct().ToArray();
        var skuMap = (await skus.GetByIdsAsync(skuIds, ct)).ToDictionary(s => s.Id);
        var spuMap = (await spus.GetByIdsAsync(skuMap.Values.Select(s => s.SpuId).Distinct(), ct))
            .ToDictionary(s => s.Id);
        var shopIds = items.Select(i => i.ShopId).Distinct().Order().ToArray();
        var shopMap = (await shops.GetByIdsAsync(shopIds, ct)).ToDictionary(s => s.Id);
        var stockMap = await inventories.GetBySkuIdsAsync(skuIds, ct);

        foreach (var item in items)
        {
            if (!skuMap.TryGetValue(item.SkuId, out var sku) || !sku.IsActive
                || sku.DeletedAt is not null || sku.ShopId != item.ShopId)
                return CheckoutErrors.SkuUnavailable(item.SkuId);
            if (!spuMap.TryGetValue(sku.SpuId, out var spu) || spu.Status != ProductStatus.Active
                || spu.DeletedAt is not null || spu.ShopId != sku.ShopId)
                return CheckoutErrors.ProductUnavailable(sku.SpuId);
            if (!shopMap.TryGetValue(item.ShopId, out var shop) || shop.Status != ShopStatus.Active
                || shop.DeletedAt is not null)
                return CheckoutErrors.ShopUnavailable(item.ShopId);
            if (item.Quantity <= 0 || !stockMap.TryGetValue(sku.Id, out var stock)
                || stock.ShopId != item.ShopId || stock.SkuId != sku.Id || stock.AvailableQty < item.Quantity)
                return CheckoutErrors.InsufficientStock(item.SkuId);
        }

        if (selections is not null && (selections.Count != shopIds.Length
            || selections.Select(s => s.ShopId).Distinct().Count() != selections.Count
            || shopIds.Any(id => !selections.Any(s => s.ShopId == id))))
            return OrderErrors.ShippingProviderRequired;
        var now = clock.UtcNow;
        var flashItems = await flashSales.GetActiveItemsBySkuIdsAsync(skuIds, now, ct);
        if (flashItems.GroupBy(i => i.SkuId).Any(g => g.Count() != 1)
            || flashItems.Any(i => !skuMap.TryGetValue(i.SkuId, out var sku) || sku.ShopId != i.ShopId))
            return FlashSalesErrors.ItemNotAvailable;
        var flashMap = flashItems.ToDictionary(i => i.SkuId);
        var shopVouchers = new Dictionary<Guid, VoucherSnapshot>();
        foreach (var selection in request.ShopVouchers ?? [])
        {
            if (!shopIds.Contains(selection.ShopId))
                return CheckoutErrors.InvalidVoucher(selection.Code, "Gian hàng không có sản phẩm được chọn.");
            var voucher = await LoadVoucherAsync(selection.Code, userId, selection.ShopId, false, now, ct);
            if (voucher.IsFailure) return voucher.Error;
            shopVouchers.Add(selection.ShopId, voucher.Value!);
        }

        VoucherSnapshot? productVoucher = null;
        if (request.PlatformProductVoucherCode is { } productCode)
        {
            var voucher = await LoadVoucherAsync(productCode, userId, null, false, now, ct);
            if (voucher.IsFailure) return voucher.Error;
            productVoucher = voucher.Value;
        }
        VoucherSnapshot? shippingVoucher = null;
        if (request.PlatformFreeshipVoucherCode is { } shippingCode)
        {
            var voucher = await LoadVoucherAsync(shippingCode, userId, null, true, now, ct);
            if (voucher.IsFailure) return voucher.Error;
            shippingVoucher = voucher.Value;
        }

        var primaryAddresses = await warehouses.GetPrimaryByShopIdsAsync(shopIds, ct);
        var shopInputs = new List<ShopCalculationInput>();
        var shippingRequests = new List<ShippingFeeCalculationModel>();
        foreach (var shopId in shopIds)
        {
            var primary = primaryAddresses.Where(a => a.ShopId == shopId && a.IsPrimary).ToArray();
            if (primary.Length != 1)
                return CheckoutErrors.PrimaryWarehouseMissing(shopId);
            var warehouse = primary[0];
            if (warehouse.DistrictId is not > 0 || string.IsNullOrWhiteSpace(warehouse.WardCode))
                return CheckoutErrors.InvalidWarehouseAddress(shopId);
            var selectedItems = items.Where(i => i.ShopId == shopId).OrderBy(i => i.Id).ToArray();
            var weight = selectedItems.Sum(i => (decimal)skuMap[i.SkuId].WeightGram * i.Quantity);
            if (weight <= 0 || weight > int.MaxValue || selectedItems.Any(i => skuMap[i.SkuId].WeightGram <= 0))
                return CheckoutErrors.InvalidWeight(shopId);
            int? serviceType = null;
            if (selections is not null)
            {
                var selection = selections.Single(s => s.ShopId == shopId);
                if (!string.Equals(selection.Provider, shipping.ProviderCode, StringComparison.OrdinalIgnoreCase)
                    || !int.TryParse(selection.ServiceCode, out var parsed) || parsed <= 0)
                    return OrderErrors.ShippingProviderRequired;
                serviceType = parsed;
            }
            shippingRequests.Add(new ShippingFeeCalculationModel(
                warehouse.DistrictId.Value, warehouse.WardCode, address.DistrictId.Value,
                address.WardCode, (int)weight, ServiceTypeId: serviceType));
            shopInputs.Add(new ShopCalculationInput(shopId, 0m, selectedItems.Select(i =>
            {
                var sku = skuMap[i.SkuId];
                return new OrderItemCalculationInput(sku.Id, sku.SpuId, spuMap[sku.SpuId].CategoryId,
                    i.Quantity, flashMap.TryGetValue(sku.Id, out var flash) ? flash.FlashPrice : sku.SellPrice);
            }).ToList(), shopVouchers.GetValueOrDefault(shopId)));
        }

        // Only provider I/O runs in parallel; all repository reads have completed.
        var quotes = await Task.WhenAll(shippingRequests.Select((model, index) =>
            CalculateShippingAsync(shopIds[index], model, ct)));
        ct.ThrowIfCancellationRequested();
        for (var i = 0; i < quotes.Length; i++)
        {
            if (quotes[i].IsFailure) return quotes[i].Error;
            if (selections is not null && (!string.Equals(quotes[i].Value!.ProviderCode,
                    selections.Single(s => s.ShopId == shopIds[i]).Provider, StringComparison.OrdinalIgnoreCase)
                || quotes[i].Value!.ServiceCode != selections.Single(s => s.ShopId == shopIds[i]).ServiceCode))
                return OrderErrors.ShippingProviderRequired;
            shopInputs[i] = shopInputs[i] with { ShippingFee = quotes[i].Value!.TotalFee };
        }

        var calculation = PriceCalculator.Calculate(new PriceCalculationInput(shopInputs, productVoucher, shippingVoucher, now));
        if (calculation.IsFailure) return calculation.Error;
        var result = calculation.Value!;
        var rejectedVoucher = result.VoucherEvaluations.FirstOrDefault(v => !v.IsApplied);
        if (rejectedVoucher is not null)
            return CheckoutErrors.InvalidVoucher(rejectedVoucher.VoucherCode,
                $"{rejectedVoucher.ErrorCode}: {rejectedVoucher.ErrorMessage}");

        var vouchers = shopVouchers.Values.ToList();
        if (productVoucher is not null) vouchers.Add(productVoucher);
        if (shippingVoucher is not null) vouchers.Add(shippingVoucher);
        return new PreparedCheckout(userId, items, address, primaryAddresses.ToDictionary(a => a.ShopId),
            shopMap, skuMap, spuMap, vouchers, result, flashMap);
    }

    private async Task<Result<VoucherSnapshot>> LoadVoucherAsync(string code, Guid userId, Guid? shopId,
        bool isShippingVoucher, DateTimeOffset now, CancellationToken ct)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var discount = await discounts.GetByCodeAsync(normalizedCode, ct);
        if (discount is null)
            return CheckoutErrors.InvalidVoucher(normalizedCode, "Mã voucher không tồn tại.");
        var usageCount = await discounts.GetUserUsageCountAsync(discount.Id, userId, ct);
        var availability = VoucherEligibilityEvaluator.EvaluateAvailability(discount, usageCount, shopId, isShippingVoucher, now);
        if (availability.IsFailure)
            return CheckoutErrors.InvalidVoucher(normalizedCode, $"{availability.Error.Code}: {availability.Error.Description}");
        return availability.Value!;
    }

    private async Task<Result<ShippingFeeQuote>> CalculateShippingAsync(Guid shopId, ShippingFeeCalculationModel model, CancellationToken ct)
    {
        try
        {
            var quote = await shipping.CalculateFeeAsync(model, ct);
            ct.ThrowIfCancellationRequested();
            if (quote.IsFailure) return quote.Error;
            if (quote.Value is null) return CheckoutErrors.ShippingFailed(shopId);
            return quote.Value;
        }
        catch (HttpRequestException)
        {
            return CheckoutErrors.ShippingFailed(shopId);
        }
        catch (TimeoutException)
        {
            return CheckoutErrors.ShippingFailed(shopId);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return CheckoutErrors.ShippingFailed(shopId);
        }
    }
}
