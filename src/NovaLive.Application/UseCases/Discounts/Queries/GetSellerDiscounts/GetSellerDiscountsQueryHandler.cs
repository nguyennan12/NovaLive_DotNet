using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Discounts;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Discounts.Queries.GetSellerDiscounts;

public sealed class GetSellerDiscountsQueryHandler(
    IDiscountRepository discountRepository,
    ICurrentUser currentUser)
    : IQueryHandler<GetSellerDiscountsQuery, PagedResult<DiscountResponse>>
{
    public async Task<Result<PagedResult<DiscountResponse>>> Handle(GetSellerDiscountsQuery query, CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return DiscountsErrors.UnauthorizedShop;
        }

        var pagedRequest = new PagedRequest(query.Page, query.Size);

        var (items, total) = await discountRepository.GetSellerDiscountsAsync(
            shopId.Value,
            query.Status,
            pagedRequest.Skip,
            pagedRequest.Take,
            DateTimeOffset.UtcNow,
            ct);

        var dtos = items.Select(d => d.ToResponse()).ToList();

        return PagedResult<DiscountResponse>.Create(
            dtos,
            pagedRequest.NormalizedPage,
            pagedRequest.NormalizedSize,
            total);
    }
}
