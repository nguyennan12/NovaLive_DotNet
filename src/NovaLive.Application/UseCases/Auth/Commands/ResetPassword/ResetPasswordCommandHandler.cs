using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Application.UseCases.Auth.Common;
using NovaLive.Application.UseCases.Auth.Errors;

namespace NovaLive.Application.UseCases.Auth.Commands.ResetPassword;

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
