using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Users;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Users.Addresses.Commands.CreateUserAddress;

public record CreateUserAddressCommand(CreateAddressRequest Request)
    : ICommand<AddressResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Users.ManageOwnAddresses;
}
