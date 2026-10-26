using System.Text.Json;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Shops.Commands.BanShop;

public sealed class BanShopCommandHandler(
    IShopRepository shops,
    ISpuRepository spus,
    IAuditLogRepository auditLogs,
    ICurrentUser currentUser,
    IDateTimeProvider clock) : ICommandHandler<BanShopCommand, ShopResponse>
{
    public async Task<Result<ShopResponse>> Handle(BanShopCommand command, CancellationToken ct)
    {
        var adminId = currentUser.UserId ?? Guid.Empty;
        var shop = await shops.GetTrackedByIdAsync(command.ShopId, ct);
        if (shop is null)
        {
            return ShopErrors.NotFound;
        }

        var previousStatus = shop.Status;
        var banResult = shop.Ban();
        if (banResult.IsFailure)
        {
            return banResult.Error;
        }

        var now = clock.UtcNow;
        var reason = command.Request.Reason;

        await auditLogs.AddAsync(AuditLog.Create(
            adminId,
            "Shop.Banned",
            "Shop",
            shop.Id,
            JsonSerializer.Serialize(new { status = previousStatus.ToString() }),
            JsonSerializer.Serialize(new { status = shop.Status.ToString(), reason }),
            now), ct);

        // Automatically set all SPUs to Inactive to hide from public search and prevent new orders
        await spus.DeactivateByShopIdAsync(shop.Id, ct);

        var verification = await shops.GetLatestVerificationAsync(shop.Id, ct);
        var followersCount = await shops.CountFollowersAsync(shop.Id, ct);

        return shop.ToResponse(followersCount, verification);
    }
}
