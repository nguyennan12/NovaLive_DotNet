using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Users;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Users.Addresses.Commands.UpdateUserAddress;

public record UpdateUserAddressCommand(Guid AddressId, UpdateAddressRequest Request)
    : ICommand<AddressResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Users.ManageOwnAddresses;
}
