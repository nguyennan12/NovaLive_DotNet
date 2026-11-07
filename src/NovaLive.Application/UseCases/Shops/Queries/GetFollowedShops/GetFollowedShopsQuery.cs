using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;

namespace NovaLive.Application.UseCases.Shops.Queries.GetFollowedShops;

public record GetFollowedShopsQuery() : IQuery<List<FollowedShopResponse>>;
