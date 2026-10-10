using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Application.UseCases.Auth.Common;
using NovaLive.Application.UseCases.Auth.Errors;

namespace NovaLive.Application.UseCases.Auth.Commands.VerifyOtp;

public sealed class VerifyOtpCommandHandler(IAppDbContext db, IAuthPersistence persistence, IDateTimeProvider clock,
    OtpFlowService otps) : ICommandHandler<VerifyOtpCommand>
{
    public async Task<Result> Handle(VerifyOtpCommand request, CancellationToken ct)
    {
        await persistence.LockUserAsync(request.Data.UserId, ct);
        var user = await db.Users.SingleOrDefaultAsync(user => user.Id == request.Data.UserId && user.DeletedAt == null, ct);
        if (user is null || user.AccountStatus != AccountStatus.Unverified) return AuthErrors.InvalidOtp;
        var error = await otps.VerifyAsync(user.Id, OtpType.EmailVerify, request.Data.OtpCode, ct);
        if (error is not null) return error;
        user.Activate();
        if (!await db.UserRoles.AnyAsync(role => role.UserId == user.Id && role.RoleId == SystemRoleIds.Buyer, ct))
            db.UserRoles.Add(new UserRole(user.Id, SystemRoleIds.Buyer, clock.UtcNow));
        return Result.Success();
    }
}
