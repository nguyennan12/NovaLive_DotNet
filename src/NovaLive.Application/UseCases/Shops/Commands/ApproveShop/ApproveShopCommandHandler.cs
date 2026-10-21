using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops.Commands.ApproveShop;

public sealed class ApproveShopCommandHandler(
    IShopRepository shops,
    IUserRepository users,
    IShopWalletRepository wallets,
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider clock) : ICommandHandler<ApproveShopCommand, ShopResponse>
{
    public async Task<Result<ShopResponse>> Handle(ApproveShopCommand command, CancellationToken ct)
    {
        var adminId = currentUser.UserId ?? Guid.Empty;
        var shop = await shops.GetTrackedByIdAsync(command.ShopId, ct);
        if (shop is null)
        {
            return ShopErrors.NotFound;
        }

        var verification = await shops.GetLatestTrackedVerificationAsync(shop.Id, ct);
        if (verification is null || verification.Status != VerificationStatus.Pending)
        {
            return ShopErrors.VerificationNotPending;
        }

        var now = clock.UtcNow;

        // 1. Approve shop
        shop.Approve();

        // 2. Approve latest verification
        verification.Approve(adminId, command.Request.Note, now);

        // 3. Mark user as seller & assign Seller role
        var owner = await users.GetTrackedByIdAsync(shop.OwnerId, ct);
        if (owner is not null)
        {
            owner.MarkAsSeller();
            if (!await dbContext.UserRoles.AnyAsync(r => r.UserId == owner.Id && r.RoleId == SystemRoleIds.Seller, ct))
            {
                await dbContext.UserRoles.AddAsync(new UserRole(owner.Id, SystemRoleIds.Seller, now), ct);
            }
        }

        // 4. Initialize ShopWallet with balance = 0, locked_balance = 0 if not already existing
        var wallet = await wallets.GetByShopIdAsync(shop.Id, ct);
        if (wallet is null)
        {
            wallet = new ShopWallet(shop.Id);
            await wallets.AddAsync(wallet, ct);
        }

        var followersCount = await shops.CountFollowersAsync(shop.Id, ct);
        return shop.ToResponse(followersCount, verification);
    }
}
