using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;

namespace NovaLive.Application.UseCases.Shops.Queries.GetShopPublic;

public sealed record GetShopPublicQuery(Guid ShopId) : IQuery<ShopDetailResponse>;
