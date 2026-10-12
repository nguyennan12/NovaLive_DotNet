using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Users;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Users.Addresses.Queries.GetUserAddresses;

public record GetUserAddressesQuery : IQuery<IReadOnlyList<AddressResponse>>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Users.ManageOwnAddresses;
}
