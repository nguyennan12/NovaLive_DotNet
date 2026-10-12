using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Discounts;

namespace NovaLive.Application.UseCases.Discounts.Queries.GetShopDiscounts;

public record GetShopDiscountsQuery(Guid ShopId) : IQuery<IReadOnlyList<DiscountResponse>>;
