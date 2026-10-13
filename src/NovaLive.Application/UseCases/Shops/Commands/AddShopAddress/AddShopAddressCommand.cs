using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops.Commands.AddShopAddress;

[RequireAuthenticated]
public sealed record AddShopAddressCommand(CreateShopAddressRequest Request) : ICommand<ShopAddressResponse>, ITransactionalCommand;
