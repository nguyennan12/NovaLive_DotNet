using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Users;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Users.Profile.Queries.GetUserProfile;

public record GetUserProfileQuery : IQuery<UserProfileResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Users.ViewOwnProfile;
}
