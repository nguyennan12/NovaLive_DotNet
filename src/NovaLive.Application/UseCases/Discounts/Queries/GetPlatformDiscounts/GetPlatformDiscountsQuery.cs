using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Discounts;

namespace NovaLive.Application.UseCases.Discounts.Queries.GetPlatformDiscounts;

public record GetPlatformDiscountsQuery : IQuery<IReadOnlyList<DiscountResponse>>;
