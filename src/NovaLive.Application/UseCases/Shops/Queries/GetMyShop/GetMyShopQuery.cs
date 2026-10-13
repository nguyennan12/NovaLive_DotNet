using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;

namespace NovaLive.Application.UseCases.Shops.Queries.GetMyShop;

[RequireAuthenticated]
public sealed record GetMyShopQuery() : IQuery<ShopResponse>;
