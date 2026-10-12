using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Application.UseCases.Auth.Common;

namespace NovaLive.Application.UseCases.Auth.Commands.ForgotPassword;

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
