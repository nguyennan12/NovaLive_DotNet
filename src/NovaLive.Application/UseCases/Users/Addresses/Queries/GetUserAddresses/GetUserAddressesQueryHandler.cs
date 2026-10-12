using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Users.Common;
using NovaLive.Contracts.V1.Users;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Users.Addresses.Queries.GetUserAddresses;

public sealed class GetUserAddressesQueryHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : IQueryHandler<GetUserAddressesQuery, IReadOnlyList<AddressResponse>>
{
    public async Task<Result<IReadOnlyList<AddressResponse>>> Handle(GetUserAddressesQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue || userId == Guid.Empty)
        {
            return UserErrors.Unauthorized;
        }

        var addresses = await db.UserAddresses
            .AsNoTracking()
            .Where(a => a.UserId == userId.Value)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new AddressResponse(
                a.Id,
                a.UserId,
                a.RecipientName,
                a.Phone,
                a.ProvinceId,
                a.ProvinceName,
                a.DistrictId,
                a.DistrictName,
                a.WardCode,
                a.WardName,
                a.DetailAddress,
                a.IsDefault,
                a.CreatedAt))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<AddressResponse>>(addresses);
    }
}
