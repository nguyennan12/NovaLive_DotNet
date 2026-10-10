using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
namespace NovaLive.Application.Auth;

public sealed class RegisterUserCommandHandler(IAppDbContext db, IAuthPersistence persistence, IPasswordHasher passwords,
    IDateTimeProvider clock, OtpFlowService otps) : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<Result<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var data = request.Data;
        var email = AuthSessionService.Normalize(data.Email);
        var phone = AuthSessionService.NormalizePhone(data.Phone);
        if (await db.Users.AnyAsync(user => user.DeletedAt == null && (user.Email == email || user.Phone == phone), ct))
            return AuthErrors.Duplicate;
        var user = new User(email, passwords.Hash(data.Password), data.FullName.Trim(), phone)
            { CreatedAt = clock.UtcNow, UpdatedAt = clock.UtcNow };
        db.Users.Add(user);
        var record = await otps.IssueAsync(user, OtpType.EmailVerify, ct);
        var error = await persistence.SaveRegistrationAsync(ct);
        return error is not null ? error :
            new RegisterUserResponse(user.Id, user.Email, user.AccountStatus.ToString(), record.ExpiresAt.UtcDateTime);
    }
}
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

