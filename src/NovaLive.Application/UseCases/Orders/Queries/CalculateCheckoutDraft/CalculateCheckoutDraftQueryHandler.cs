using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Pricing;

namespace NovaLive.Application.UseCases.Orders.Queries.CalculateCheckoutDraft;

public sealed class CalculateCheckoutDraftQueryHandler(
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
    : IQueryHandler<CalculateCheckoutDraftQuery, CheckoutPreviewResponse>
{
    public async Task<Result<CheckoutPreviewResponse>> Handle(CalculateCheckoutDraftQuery query, CancellationToken ct)
    {
        var result = await new CheckoutDraftBuilder(carts, skus, spus, shops, inventories, addresses,
            warehouses, discounts, shipping, currentUser, clock, flashSales).BuildAsync(query.Request, ct);
        if (result.IsFailure) return result.Error;
        var draft = result.Value!;
        return draft.Price.ToPreview(draft.Shops, draft.Skus, draft.Spus);
    }
}
