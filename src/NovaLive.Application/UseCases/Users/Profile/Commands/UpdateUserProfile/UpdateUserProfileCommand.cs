using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Users;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Users.Profile.Commands.UpdateUserProfile;

public record UpdateUserProfileCommand(UpdateProfileRequest Request)
    : ICommand<UserProfileResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Users.UpdateOwnProfile;
}
