using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Users.Addresses.Commands.DeleteUserAddress;

public record DeleteUserAddressCommand(Guid AddressId)
    : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Users.ManageOwnAddresses;
}
