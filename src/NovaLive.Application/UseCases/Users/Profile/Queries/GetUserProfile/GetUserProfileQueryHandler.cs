using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Users.Common;
using NovaLive.Contracts.V1.Users;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Users.Profile.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : IQueryHandler<GetUserProfileQuery, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(GetUserProfileQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue || userId == Guid.Empty)
        {
            return UserErrors.Unauthorized;
        }

        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId.Value, ct);

        if (user is null || user.DeletedAt != null)
        {
            return UserErrors.NotFound;
        }

        if (user.AccountStatus != AccountStatus.Active)
        {
            return UserErrors.Inactive;
        }

        return new UserProfileResponse(
            user.Id,
            user.Email,
            user.Phone,
            user.FullName,
            user.Birthday,
            user.Gender?.ToString(),
            user.AvatarUrl,
            user.AccountStatus.ToString(),
            user.CreatedAt);
    }
}
