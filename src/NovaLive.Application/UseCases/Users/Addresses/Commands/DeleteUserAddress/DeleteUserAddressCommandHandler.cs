using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Users.Common;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Users.Addresses.Commands.DeleteUserAddress;

public sealed class DeleteUserAddressCommandHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : ICommandHandler<DeleteUserAddressCommand>
{
    public async Task<Result> Handle(DeleteUserAddressCommand command, CancellationToken ct)
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

        if (address.IsDefault)
        {
            var totalAddresses = await db.UserAddresses
                .CountAsync(a => a.UserId == userId.Value, ct);

            if (totalAddresses > 1)
            {
                return UserAddressErrors.CannotDeleteDefaultAddress;
            }
        }

        db.UserAddresses.Remove(address);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
