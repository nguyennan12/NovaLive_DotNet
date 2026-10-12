using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Users.Common;
using NovaLive.Contracts.V1.Users;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Users.Profile.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : ICommandHandler<UpdateUserProfileCommand, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(UpdateUserProfileCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue || userId == Guid.Empty)
        {
            return UserErrors.Unauthorized;
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value, ct);
        if (user is null || user.DeletedAt != null)
        {
            return UserErrors.NotFound;
        }

        if (user.AccountStatus != AccountStatus.Active)
        {
            return UserErrors.Inactive;
        }

        var req = command.Request;

        // Check phone uniqueness if phone is updated
        if (!string.IsNullOrWhiteSpace(req.Phone) && req.Phone != user.Phone)
        {
            var phoneExists = await db.Users.AnyAsync(
                u => u.Phone == req.Phone && u.Id != user.Id && u.DeletedAt == null,
                ct);

            if (phoneExists)
            {
                return UserErrors.PhoneAlreadyInUse;
            }
        }

        UserGender? gender = null;
        if (!string.IsNullOrWhiteSpace(req.Gender) && Enum.TryParse<UserGender>(req.Gender, true, out var parsedGender))
        {
            gender = parsedGender;
        }

        user.UpdateProfile(
            req.FullName,
            string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone,
            req.Birthday,
            gender,
            string.IsNullOrWhiteSpace(req.AvatarUrl) ? null : req.AvatarUrl);

        await db.SaveChangesAsync(ct);

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
