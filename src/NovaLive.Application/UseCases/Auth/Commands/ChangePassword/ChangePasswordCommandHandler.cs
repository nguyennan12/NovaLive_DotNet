using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Application.UseCases.Auth.Common;
using NovaLive.Application.UseCases.Auth.Errors;

namespace NovaLive.Application.UseCases.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(IAppDbContext db, IAuthPersistence persistence, ICurrentUser currentUser,
    IPasswordHasher passwords, IDateTimeProvider clock, AuthSessionService sessions) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is not Guid userId) return AuthErrors.InvalidCredentials;
        await persistence.LockUserAsync(userId, ct);
        var user = await db.Users.SingleOrDefaultAsync(item => item.Id == userId, ct);
        if (user is null || user.AccountStatus != AccountStatus.Active || user.DeletedAt != null) return AuthErrors.Inactive;
        if (!passwords.Verify(request.Data.OldPassword, user.PasswordHash))
            return AuthErrors.InvalidPassword;
        if (passwords.Verify(request.Data.NewPassword, user.PasswordHash))
            return AuthErrors.PasswordUnchanged;
        user.ChangePassword(passwords.Hash(request.Data.NewPassword));
        await persistence.RevokeAllAsync(user.Id, clock.UtcNow, ct);
        await sessions.BlacklistCurrentAsync(ct);
        return Result.Success();
    }
}
