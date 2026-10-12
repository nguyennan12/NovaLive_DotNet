using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Users.Common;
using NovaLive.Contracts.V1.Users;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Users.Addresses.Commands.UpdateUserAddress;

public sealed class UpdateUserAddressCommandHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : ICommandHandler<UpdateUserAddressCommand, AddressResponse>
{
    public async Task<Result<AddressResponse>> Handle(UpdateUserAddressCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue || userId == Guid.Empty)
        {
            return UserErrors.Unauthorized;
        }

        var address = await db.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == command.AddressId && a.UserId == userId.Value, ct);

        if (address is null)
        {
            return UserAddressErrors.NotFound;
        }

        var req = command.Request;
        var isDefault = req.IsDefault;

        if (isDefault)
        {
            var otherDefaults = await db.UserAddresses
                .Where(a => a.UserId == userId.Value && a.Id != address.Id && a.IsDefault)
                .ToListAsync(ct);

            foreach (var other in otherDefaults)
            {
                other.UnsetDefault();
            }
        }
        else if (address.IsDefault)
        {
            // If this is the only address, it must remain default
            var totalCount = await db.UserAddresses.CountAsync(a => a.UserId == userId.Value, ct);
            if (totalCount <= 1)
            {
                isDefault = true;
            }
        }

        address.UpdateDetails(
            req.RecipientName,
            req.Phone,
            req.ProvinceId,
            req.ProvinceName,
            req.DistrictId,
            req.DistrictName,
            req.WardCode,
            req.WardName,
            req.DetailAddress,
            isDefault);

        await db.SaveChangesAsync(ct);

        return new AddressResponse(
            address.Id,
            address.UserId,
            address.RecipientName,
            address.Phone,
            address.ProvinceId,
            address.ProvinceName,
            address.DistrictId,
            address.DistrictName,
            address.WardCode,
            address.WardName,
            address.DetailAddress,
            address.IsDefault,
            address.CreatedAt);
    }
}
