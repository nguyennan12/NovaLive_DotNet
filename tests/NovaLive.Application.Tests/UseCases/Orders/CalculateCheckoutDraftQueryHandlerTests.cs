using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Carts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Products;
using NovaLive.Domain.Users;

namespace NovaLive.Application.Tests.UseCases.Orders;

public sealed class CalculateCheckoutDraftQueryHandlerTests
{
    [Fact]
    public async Task MultipleSkusInOneShop_ShareOneShippingQuoteWithSummedWeight()
    {
        using var c = new CheckoutTestContext();
        var shop = c.ShopList[0]; var spu = c.SpuList[0];
        var sku = new Sku(spu.Id, shop.Id, "SECOND", null, 50m, 40m, 300);
        c.SkuList.Add(sku);
        c.Stocks.Add(sku.Id, new NovaLive.Domain.Inventory.Inventory(sku.Id, shop.Id, 10));
        c.Items.Add(new CartItem(Guid.NewGuid(), c.Cart.Id, sku.Id, shop.Id, 3, 1m));
        var result = await c.Run();
        result.IsSuccess.Should().BeTrue();
        result.Value!.ShopPreviews.Should().ContainSingle().Which.Items.Should().HaveCount(2);
        result.Value.TotalItemsAmount.Should().Be(320m);
        result.Value.GrandTotal.Should().Be(350m);
        c.Shipping.Verify(x => x.CalculateFeeAsync(It.Is<ShippingFeeCalculationModel>(m => m.WeightGram == 1300),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1.5)]
    public async Task InvalidProviderAmounts_AreDomainFailures(decimal fee)
    {
        using var c = new CheckoutTestContext();
        c.Shipping.Setup(x => x.CalculateFeeAsync(It.IsAny<ShippingFeeCalculationModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ShippingFeeQuote("GHN", "2", fee)));
        (await c.Run()).Error.Code.Should().Be(fee < 0 ? "Pricing.NegativeShippingFee" : "Pricing.NonIntegerAmount");
    }

    [Fact]
    public async Task OneShop_UsesCurrentSellPriceAndOnlySelectedItems_WithoutMutatingState()
    {
        using var c = new CheckoutTestContext();
        var item = c.Items[0];
        c.AddShop();
        c.Stocks[item.SkuId].Reserve(3);
        var result = await c.Run(c.Request with { CartItemIds = [item.Id] });
        result.IsSuccess.Should().BeTrue();
        var preview = result.Value!;
        preview.ShopPreviews.Should().ContainSingle();
        preview.TotalItemsAmount.Should().Be(200m);
        preview.TotalShippingFee.Should().Be(30m);
        preview.GrandTotal.Should().Be(230m);
        preview.ShopPreviews.Single().Items.Single().Should().BeEquivalentTo(new CheckoutItemDto(
            item.SkuId, "Product", "SKU", "{}", "thumbnail", 100m, 2, 200m));
        c.Items.Should().HaveCount(2);
        item.UnitPrice.Should().Be(1m);
        item.UpdatedAt.Should().Be(c.Now.AddDays(-1));
        c.Stocks[item.SkuId].ReservedQty.Should().Be(3);
        c.Stocks[item.SkuId].QtyOnHand.Should().Be(10);
        c.Cart.UpdatedAt.Should().Be(c.Now.AddDays(-1));
        c.Shipping.Verify(x => x.CalculateFeeAsync(It.Is<ShippingFeeCalculationModel>(m =>
            m.FromDistrictId == 100 && m.FromWardCode == "W0" && m.ToDistrictId == 200
            && m.ToWardCode == "W200" && m.WeightGram == 400 && m.ServiceTypeId == null), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TwoShops_QuoteConcurrentlyUsingTheirPrimaryWarehouses_AndForwardCancellation()
    {
        using var c = new CheckoutTestContext(); c.AddShop(50m, 3);
        var allEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var count = 0;
        using var source = new CancellationTokenSource(); var ct = source.Token;
        c.Shipping.Setup(x => x.CalculateFeeAsync(It.IsAny<ShippingFeeCalculationModel>(), ct))
            .Returns(async (ShippingFeeCalculationModel model, CancellationToken token) =>
            {
                if (Interlocked.Increment(ref count) == 2) allEntered.SetResult();
                await allEntered.Task.WaitAsync(TimeSpan.FromSeconds(5), token);
                return Result.Success(new ShippingFeeQuote("GHN", "2", model.FromDistrictId == 100 ? 30m : 40m));
            });
        var result = await c.Run(ct: ct);
        result.IsSuccess.Should().BeTrue();
        result.Value!.ShopPreviews.Should().HaveCount(2);
        result.Value.TotalItemsAmount.Should().Be(350m);
        result.Value.TotalShippingFee.Should().Be(70m);
        result.Value.GrandTotal.Should().Be(420m);
        foreach (var warehouse in c.WarehouseList)
        {
            var sku = c.SkuList.Single(s => s.ShopId == warehouse.ShopId);
            var item = c.Items.Single(i => i.SkuId == sku.Id);
            c.Shipping.Verify(x => x.CalculateFeeAsync(It.Is<ShippingFeeCalculationModel>(m =>
                m.FromDistrictId == warehouse.DistrictId && m.FromWardCode == warehouse.WardCode
                && m.WeightGram == sku.WeightGram * item.Quantity), ct), Times.Once);
        }
        new[] { c.Carts.Invocations, c.Skus.Invocations, c.Spus.Invocations, c.Shops.Invocations,
            c.Inventories.Invocations, c.Addresses.Invocations, c.Warehouses.Invocations }
            .SelectMany(i => i).Should().OnlyContain(i => i.Arguments.Last().Equals(ct));
    }

    [Theory]
    [InlineData("missing-cart")]
    [InlineData("foreign-cart")]
    [InlineData("missing-item")]
    [InlineData("foreign-item")]
    [InlineData("unexpected-item")]
    public async Task ItemsMustAllBelongToTheCurrentUsersCart(string scenario)
    {
        using var c = new CheckoutTestContext();
        if (scenario == "missing-cart")
            c.Carts.Setup(x => x.GetByUserIdAsync(c.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart?)null);
        if (scenario == "foreign-cart")
            c.Carts.Setup(x => x.GetByUserIdAsync(c.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new Cart(Guid.NewGuid(), c.Now));
        if (scenario == "missing-item") c.Items.Clear();
        if (scenario is "foreign-item" or "unexpected-item")
        {
            var own = c.Items[0];
            c.Carts.Setup(x => x.GetCartItemsByIdsAsync(It.IsAny<IEnumerable<Guid>>(), c.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync([new CartItem(scenario == "foreign-item" ? own.Id : Guid.NewGuid(),
                    scenario == "foreign-item" ? Guid.NewGuid() : c.Cart.Id, own.SkuId, own.ShopId, 2, 1m)]);
        }
        var request = c.Request with { CartItemIds = scenario == "missing-item" ? [Guid.NewGuid()] : c.Request.CartItemIds };
        (await c.Run(request)).Error.Should().Be(CheckoutErrors.CartItemsNotFound);
        c.Shipping.Invocations.Should().BeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingOrForeignAddress_IsNotFound(bool foreign)
    {
        using var c = new CheckoutTestContext();
        c.Addresses.Setup(x => x.GetByIdForUserAsync(c.Address.Id, c.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(foreign ? new UserAddress(Guid.NewGuid(), "Other", "0", 1, "P", 1, "D", "W", "W", "S") : null);
        (await c.Run()).Error.Should().Be(CheckoutErrors.AddressNotFound);
        c.Shipping.Invocations.Should().BeEmpty();
    }

    [Theory]
    [InlineData("sku-inactive", "Checkout.SkuUnavailable")]
    [InlineData("sku-deleted", "Checkout.SkuUnavailable")]
    [InlineData("sku-missing", "Checkout.SkuUnavailable")]
    [InlineData("spu-inactive", "Checkout.ProductUnavailable")]
    [InlineData("spu-draft", "Checkout.ProductUnavailable")]
    [InlineData("spu-banned", "Checkout.ProductUnavailable")]
    [InlineData("spu-deleted", "Checkout.ProductUnavailable")]
    [InlineData("spu-missing", "Checkout.ProductUnavailable")]
    [InlineData("shop-suspended", "Checkout.ShopUnavailable")]
    [InlineData("shop-banned", "Checkout.ShopUnavailable")]
    [InlineData("shop-deleted", "Checkout.ShopUnavailable")]
    [InlineData("shop-missing", "Checkout.ShopUnavailable")]
    [InlineData("insufficient-stock", "Checkout.InsufficientStock")]
    [InlineData("missing-stock", "Checkout.InsufficientStock")]
    [InlineData("no-primary", "Checkout.PrimaryWarehouseMissing")]
    [InlineData("non-primary", "Checkout.PrimaryWarehouseMissing")]
    [InlineData("invalid-warehouse", "Checkout.InvalidWarehouseAddress")]
    [InlineData("invalid-address", "Checkout.IncompleteAddress")]
    [InlineData("weight-overflow", "Checkout.InvalidWeight")]
    public async Task UnavailableCatalogStockAndShippingData_ReturnBusinessErrors(string scenario, string code)
    {
        using var c = new CheckoutTestContext();
        switch (scenario)
        {
            case "sku-inactive": c.SkuList[0].SetActive(false); break;
            case "sku-deleted": c.SkuList[0].SoftDelete(); break;
            case "sku-missing": c.SkuList.Clear(); break;
            case "spu-inactive": c.SpuList[0].SetStatus(ProductStatus.Inactive); break;
            case "spu-draft": c.SpuList[0].SetStatus(ProductStatus.Draft); break;
            case "spu-banned": c.SpuList[0].SetStatus(ProductStatus.Banned); break;
            case "spu-deleted": c.SpuList[0].SoftDelete(); break;
            case "spu-missing": c.SpuList.Clear(); break;
            case "shop-suspended": c.ShopList[0].Suspend(); break;
            case "shop-banned": typeof(NovaLive.Domain.Shops.Shop).GetProperty("Status")!.SetValue(c.ShopList[0], ShopStatus.Banned); break;
            case "shop-deleted": typeof(NovaLive.Domain.Shops.Shop).GetProperty("DeletedAt")!.SetValue(c.ShopList[0], c.Now); break;
            case "shop-missing": c.ShopList.Clear(); break;
            case "insufficient-stock": c.Stocks[c.Items[0].SkuId].Reserve(9); break;
            case "missing-stock": c.Stocks.Clear(); break;
            case "no-primary": c.WarehouseList.Clear(); break;
            case "non-primary": c.WarehouseList[0].SetPrimary(false); break;
            case "invalid-warehouse": typeof(NovaLive.Domain.Shops.ShopAddress).GetProperty("WardCode")!.SetValue(c.WarehouseList[0], ""); break;
            case "invalid-address": c.Address.UpdateDetails("Buyer", "0", 1, "P", null, "D", null, "W", "Street", false); break;
            case "weight-overflow": c.SkuList[0].UpdatePriceAndDetails(100m, 100m, int.MaxValue, true); break;
        }
        var result = await c.Run();
        result.IsFailure.Should().BeTrue(); result.Error.Code.Should().Be(code);
        c.Shipping.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task ThreeVoucherTiers_AreDelegatedToDomain_AndProratedToTheExactDong()
    {
        using var c = new CheckoutTestContext(); c.AddShop(100m, 1);
        var shop = c.AddVoucher("SHOP", c.ShopList[0].Id, value: 30m);
        var platform = c.AddVoucher("PLATFORM", value: 101m);
        var shipping = c.AddVoucher("SHIP", type: DiscountType.FreeShip, value: 17m);
        var usedCounts = c.VoucherList.Select(v => v.UsedCount).ToArray();
        var updatedAt = c.VoucherList.Select(v => v.UpdatedAt).ToArray();
        using var source = new CancellationTokenSource(); var ct = source.Token;
        var result = await c.Run(c.Request with {
            ShopVouchers = [new(shop.ShopId!.Value, " shop ")],
            PlatformProductVoucherCode = " platform ", PlatformFreeshipVoucherCode = "ship"
        }, ct);
        result.IsSuccess.Should().BeTrue();
        var preview = result.Value!;
        preview.TotalShopDiscount.Should().Be(30m);
        preview.TotalPlatformDiscount.Should().Be(101m);
        preview.TotalShippingDiscount.Should().Be(17m);
        preview.ShopPreviews.Sum(s => s.AllocatedPlatformDiscount).Should().Be(101m);
        preview.ShopPreviews.Single(s => s.ShopId == shop.ShopId).AllocatedPlatformDiscount.Should().Be(64m);
        preview.ShopPreviews.Sum(s => s.ShippingDiscount).Should().Be(17m);
        preview.GrandTotal.Should().Be(212m).And.Be(preview.ShopPreviews.Sum(s => s.ShopTotal));
        preview.ShopPreviews.Should().OnlyContain(s => s.ShopTotal == s.ItemsSubtotal + s.ShippingFee
            - s.ShopDiscount - s.AllocatedPlatformDiscount - s.ShippingDiscount);
        preview.ShopPreviews.Should().OnlyContain(s => s.Items.Sum(i => i.LineTotal)
            == s.ItemsSubtotal - s.ShopDiscount - s.AllocatedPlatformDiscount);
        c.VoucherList.Select(v => v.UsedCount).Should().Equal(usedCounts);
        c.VoucherList.Select(v => v.UpdatedAt).Should().Equal(updatedAt);
        foreach (var voucher in new[] { shop, platform, shipping })
            c.Discounts.Verify(x => x.GetUserUsageCountAsync(voucher.Id, c.UserId, ct), Times.Once);
        c.Discounts.Invocations.Should().OnlyContain(i => i.Arguments.Last().Equals(ct));
    }

    [Theory]
    [InlineData("missing", "không tồn tại")]
    [InlineData("inactive", "Voucher.Inactive")]
    [InlineData("expired", "Voucher.Expired")]
    [InlineData("future", "Voucher.NotYetActive")]
    [InlineData("exhausted", "Voucher.UsesExhausted")]
    [InlineData("user-limit", "Voucher.UserLimitReached")]
    [InlineData("minimum", "Voucher.MinOrderAmountNotMet")]
    [InlineData("scope", "Pricing.PlatformScopeMismatch")]
    [InlineData("type", "Pricing.InvalidVoucherTypeForSlot")]
    [InlineData("targets", "Voucher.NoEligibleItems")]
    public async Task InvalidVoucher_ReturnsValidationWithCodeAndReason(string scenario, string reason)
    {
        using var c = new CheckoutTestContext();
        var voucher = c.AddVoucher("CODE", scenario == "scope" ? c.ShopList[0].Id : null,
            scenario == "type" ? DiscountType.FreeShip : DiscountType.FixedCart,
            minimum: scenario == "minimum" ? 10000m : 0m, active: scenario != "inactive",
            maxUses: scenario == "exhausted" ? 1 : null,
            appliesTo: scenario == "targets" ? DiscountAppliesTo.SpecificSpus : DiscountAppliesTo.AllProducts,
            targets: [Guid.NewGuid()], from: scenario == "future" ? c.Now.AddDays(1) : c.Now.AddDays(-2),
            to: scenario == "expired" ? c.Now.AddDays(-1) : c.Now.AddDays(2));
        if (scenario == "missing") c.VoucherList.Clear();
        if (scenario == "exhausted") voucher.IncrementUsedCount();
        if (scenario == "user-limit")
            c.Discounts.Setup(x => x.GetUserUsageCountAsync(voucher.Id, c.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var usedCount = voucher.UsedCount;
        var result = await c.Run(c.Request with { PlatformProductVoucherCode = "CODE" });
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("Checkout.InvalidVoucher");
        result.Error.Description.Should().Contain("CODE").And.Contain(reason);
        voucher.UsedCount.Should().Be(usedCount);
    }

    [Fact]
    public async Task ShopVoucherMustMatchTheSelectedShop_AndFreeshipMustBeAPlatformShippingVoucher()
    {
        using var c = new CheckoutTestContext();
        c.AddVoucher("WRONGSHOP", Guid.NewGuid());
        var request = c.Request with { ShopVouchers = [new(c.ShopList[0].Id, "WRONGSHOP")] };
        (await c.Run(request)).Error.Description.Should().Contain("WRONGSHOP").And.Contain("Pricing.ShopMismatch");
        (await c.Run(c.Request with { ShopVouchers = [new(Guid.NewGuid(), "UNSELECTED")] }))
            .Error.Description.Should().Contain("UNSELECTED");
        c.AddVoucher("PRODUCT");
        (await c.Run(c.Request with { PlatformFreeshipVoucherCode = "PRODUCT" })).Error.Description
            .Should().Contain("PRODUCT").And.Contain("Pricing.InvalidVoucherTypeForSlot");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvalidFreeshipIsRejectedEvenWhenShippingIsFree(bool expired)
    {
        using var c = new CheckoutTestContext();
        c.AddVoucher("SHIP", type: DiscountType.FreeShip, minimum: expired ? 0m : 1000m,
            to: expired ? c.Now.AddHours(-1) : c.Now.AddDays(1));
        c.Shipping.Setup(x => x.CalculateFeeAsync(It.IsAny<ShippingFeeCalculationModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ShippingFeeQuote("GHN", "2", 0m)));
        (await c.Run(c.Request with { PlatformFreeshipVoucherCode = "SHIP" })).Error.Description
            .Should().Contain(expired ? "Voucher.Expired" : "Voucher.MinOrderAmountNotMet");
    }

    [Fact]
    public async Task ProviderFailure_IsReturnedWithoutThrowing()
    {
        using var c = new CheckoutTestContext();
        var error = Error.Failure("Ghn.Unavailable", "Provider unavailable");
        c.Shipping.Setup(x => x.CalculateFeeAsync(It.IsAny<ShippingFeeCalculationModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ShippingFeeQuote>.Failure(error));
        (await c.Run()).Error.Should().Be(error);
    }

    [Theory]
    [InlineData("network")]
    [InlineData("timeout")]
    [InlineData("provider-cancellation")]
    public async Task ProviderTransportExceptions_AreFailures(string scenario)
    {
        using var c = new CheckoutTestContext();
        Exception exception = scenario switch {
            "network" => new HttpRequestException("offline"), "timeout" => new TimeoutException(), _ => new TaskCanceledException()
        };
        c.Shipping.Setup(x => x.CalculateFeeAsync(It.IsAny<ShippingFeeCalculationModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        (await c.Run()).Error.Code.Should().Be("Checkout.ShippingFailed");
    }

    [Fact]
    public async Task CallerCancellation_PropagatesEvenIfProviderSwallowsIt()
    {
        using var c = new CheckoutTestContext(); using var source = new CancellationTokenSource();
        c.Shipping.Setup(x => x.CalculateFeeAsync(It.IsAny<ShippingFeeCalculationModel>(), source.Token))
            .Returns((ShippingFeeCalculationModel _, CancellationToken _) =>
            {
                source.Cancel();
                return Task.FromResult(Result<ShippingFeeQuote>.Failure(Error.Failure("Ghn.Cancelled")));
            });
        var action = () => c.Run(ct: source.Token);
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UnauthenticatedUser_IsRejectedBeforeReading()
    {
        using var c = new CheckoutTestContext(); c.User.SetupGet(x => x.IsAuthenticated).Returns(false);
        (await c.Run()).Error.Should().Be(CheckoutErrors.Unauthorized);
        c.Carts.Invocations.Should().BeEmpty();
    }
}
