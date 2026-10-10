using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Application.UseCases.Auth.Common;
using NovaLive.Application.UseCases.Auth.Errors;

namespace NovaLive.Application.UseCases.Auth.Commands.Login;

public sealed class LoginCommandHandler(IAppDbContext db, IAuthPersistence persistence, IPasswordHasher passwords,
    ICacheService cache, AuthSessionService sessions) : ICommandHandler<LoginCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await sessions.FindUserAsync(request.Data.EmailOrPhone, ct);
        var email = user?.Email ?? AuthSessionService.Normalize(request.Data.EmailOrPhone);
        if (user is not null)
        {
            await persistence.LockUserAsync(user.Id, ct);
            // Query without tracking to observe password/status changes committed while waiting.
            user = await db.Users.AsNoTracking().SingleAsync(item => item.Id == user.Id, ct);
        }
        var lockKey = "login:lock:" + email;
        if (await cache.ExistsAsync(lockKey, ct))
            return AuthErrors.Locked((int)Math.Ceiling((await cache.GetTimeToLiveAsync(lockKey, ct) ?? TimeSpan.FromMinutes(5)).TotalSeconds));
        var valid = false;
        if (user is null || string.IsNullOrEmpty(user.PasswordHash)) passwords.VerifyDummy(request.Data.Password);
        else valid = passwords.Verify(request.Data.Password, user.PasswordHash);
        if (!valid)
        {
            var failures = await cache.IncrementAsync("login:failed:" + email, TimeSpan.FromDays(1), ct);
            if (failures >= 5)
            {
                var locks = await cache.IncrementAsync("login:lockcount:" + email, TimeSpan.FromDays(30), ct);
                var minutes = locks switch { 1 => 5, 2 => 15, 3 => 60, _ => 1440 };
                await cache.SetAsync(lockKey, true, TimeSpan.FromMinutes(minutes), ct);
                await cache.RemoveAsync("login:failed:" + email, ct);
                return AuthErrors.Locked(minutes * 60);
            }
            return AuthErrors.InvalidCredentials;
        }
        if (user!.AccountStatus == AccountStatus.Unverified) return AuthErrors.NotVerified;
        if (user.AccountStatus != AccountStatus.Active || user.DeletedAt != null) return AuthErrors.Inactive;
        await sessions.ResetLoginAsync(email, ct);
        return await sessions.IssueAsync(user, request.IpAddress, request.UserAgent, ct);
    }
}
