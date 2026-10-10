using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Application.UseCases.Auth.Common;

namespace NovaLive.Application.UseCases.Auth.Commands.ResendOtp;

public sealed class ResendOtpCommandHandler(AuthSessionService sessions, IAuthPersistence persistence, IAppDbContext db,
    OtpFlowService otps) : ICommandHandler<ResendOtpCommand>
{
    public async Task<Result> Handle(ResendOtpCommand request, CancellationToken ct)
    {
        var identifier = AuthSessionService.Normalize(request.Data.EmailOrPhone);
        var user = await sessions.FindUserAsync(identifier, ct);
        var error = await otps.CheckRateAsync(user?.Email ?? identifier, ct);
        if (error is not null) return error;
        if (user is null) return Result.Success();
        await persistence.LockUserAsync(user.Id, ct);
        // Re-read after the lock in case verification won the race.
        var status = await db.Users.Where(item => item.Id == user.Id).Select(item => item.AccountStatus).SingleAsync(ct);
        if (status == AccountStatus.Unverified) await otps.IssueAsync(user, OtpType.EmailVerify, ct);
        return Result.Success();
    }
}
