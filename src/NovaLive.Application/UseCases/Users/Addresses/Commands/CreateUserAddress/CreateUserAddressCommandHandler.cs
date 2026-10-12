using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Users.Common;
using NovaLive.Contracts.V1.Users;
using NovaLive.Domain.Common;
using NovaLive.Domain.Users;

namespace NovaLive.Application.UseCases.Users.Addresses.Commands.CreateUserAddress;

public sealed class CreateUserAddressCommandHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : ICommandHandler<CreateUserAddressCommand, AddressResponse>
{
    public async Task<Result<AddressResponse>> Handle(CreateUserAddressCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue || userId == Guid.Empty)
        {
            return UserErrors.Unauthorized;
        }

        var req = command.Request;

        var existingAddresses = await db.UserAddresses
            .Where(a => a.UserId == userId.Value)
            .ToListAsync(ct);

        var isFirstAddress = existingAddresses.Count == 0;
        var shouldBeDefault = req.IsDefault || isFirstAddress;

        if (shouldBeDefault)
        {
            foreach (var addr in existingAddresses)
            {
                addr.UnsetDefault();
            }
        }

        var address = new UserAddress(
            userId.Value,
            req.RecipientName,
            req.Phone,
            req.ProvinceId,
            req.ProvinceName,
            req.DistrictId,
            req.DistrictName,
            req.WardCode,
            req.WardName,
            req.DetailAddress,
            shouldBeDefault);

        await db.UserAddresses.AddAsync(address, ct);
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
