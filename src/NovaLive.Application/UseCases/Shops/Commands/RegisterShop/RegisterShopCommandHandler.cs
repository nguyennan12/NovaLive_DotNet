using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;
using NovaLive.Application.Common.Helpers;
using NovaLive.Application.Common.Events;
using NovaLive.Domain.System;
using System.Text.Json;

namespace NovaLive.Application.UseCases.Shops.Commands.RegisterShop;

public sealed class RegisterShopCommandHandler(IShopRepository shops, IUserRepository users, ICurrentUser currentUser) : ICommandHandler<RegisterShopCommand, ShopResponse>
{
    public async Task<Result<ShopResponse>> Handle(RegisterShopCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return ShopErrors.Unauthorized;
        var user = await users.GetTrackedByIdAsync(userId, ct);
        if (user is null || user.AccountStatus != AccountStatus.Active || user.DeletedAt != null)
            return ShopErrors.InactiveUser;
        // UNIQUE(owner_id) includes soft-deleted shops.
        if (await shops.ExistsByOwnerAsync(userId, ct))
            return ShopErrors.AlreadyOwned;
        var req = command.Request;
        var slug = SlugHelper.GenerateSlug(req.Slug ?? req.ShopName);
        if (await shops.ExistsBySlugAsync(slug, ct))
            return ShopErrors.SlugTaken;
        var shop = Shop.Register(userId, req.ShopName, slug, req.Description, req.Phone, req.Email, req.TaxCode);
        var verification = new ShopVerification(shop.Id, userId, req.IdCardFront, req.IdCardBack,
            req.BusinessLicense, req.BankAccount, req.BankName, req.BankBranch);
        await shops.AddAsync(shop, ct);
        await shops.AddVerificationAsync(verification, ct);
        await shops.AddRegistrationOutboxAsync(new OutboxMessage(nameof(ShopRegisteredEvent),
            JsonSerializer.Serialize(new ShopRegisteredEvent(shop.Id, userId, shop.CreatedAt))), ct);
        // Save inside the transaction so PostgreSQL unique races become the same business errors.
        var error = await shops.SaveRegistrationAsync(ct);
        if (error is not null) return error;
        return shop.ToResponse(0, verification);
    }
}
