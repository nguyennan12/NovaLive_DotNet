using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Clock;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
namespace NovaLive.Application.Auth;

public sealed class ForgotPasswordCommandHandler(IAppDbContext db, IAuthPersistence persistence,
    OtpFlowService otps) : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var email = AuthSessionService.Normalize(request.Data.Email);
        // Consume the same budget for every email, without disclosing existence, status or throttling.
        if (await otps.CheckRateAsync(email, ct) is not null) return Result.Success();
        var user = await db.Users.SingleOrDefaultAsync(item => item.Email == email && item.DeletedAt == null, ct);
        if (user is null) return Result.Success();
        await persistence.LockUserAsync(user.Id, ct);
        var status = await db.Users.Where(item => item.Id == user.Id).Select(item => item.AccountStatus).SingleAsync(ct);
        if (status == AccountStatus.Active) await otps.IssueAsync(user, OtpType.ForgotPassword, ct);
        return Result.Success();
    }
}
public sealed class ResetPasswordCommandHandler(IAppDbContext db, IAuthPersistence persistence, IPasswordHasher passwords,
    IDateTimeProvider clock, OtpFlowService otps, AuthSessionService sessions) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var email = AuthSessionService.Normalize(request.Data.Email);
        var userId = await db.Users.Where(item => item.Email == email && item.DeletedAt == null)
            .Select(item => (Guid?)item.Id).SingleOrDefaultAsync(ct);
        if (userId is null) return AuthErrors.InvalidOtp;
        await persistence.LockUserAsync(userId.Value, ct);
        var user = await db.Users.SingleAsync(item => item.Id == userId, ct);
        if (user.AccountStatus != AccountStatus.Active) return AuthErrors.InvalidOtp;
        var error = await otps.VerifyAsync(user.Id, OtpType.ForgotPassword, request.Data.OtpCode, ct);
        if (error is not null) return error;
        user.ChangePassword(passwords.Hash(request.Data.NewPassword));
        await persistence.RevokeAllAsync(user.Id, clock.UtcNow, ct);
        await sessions.ResetLoginAsync(email, ct);
        return Result.Success();
    }
}
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
            return new Error(ErrorType.Invalid, "Auth.InvalidPassword", "Current password is incorrect.");
        if (passwords.Verify(request.Data.NewPassword, user.PasswordHash))
            return new Error(ErrorType.Invalid, "Auth.PasswordUnchanged", "New password must differ from current password.");
        user.ChangePassword(passwords.Hash(request.Data.NewPassword));
        await persistence.RevokeAllAsync(user.Id, clock.UtcNow, ct);
        await sessions.BlacklistCurrentAsync(ct);
        return Result.Success();
    }
}

