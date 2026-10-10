using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Cache;
using NovaLive.Application.Abstractions.Clock;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Domain.Common;
using NovaLive.Domain.Users;
namespace NovaLive.Application.Auth;

public sealed class OtpFlowService(IAppDbContext db, IOtpService otp, ICacheService cache,
    IDateTimeProvider clock, IEmailSender email, IAfterCommitActions afterCommit, ILogger<OtpFlowService> logger)
{
    public async Task<Error?> CheckRateAsync(string address, CancellationToken ct)
    {
        var key = "otp:rate:" + address;
        var count = await cache.IncrementAsync(key, TimeSpan.FromMinutes(15), ct);
        return count > 3 ? AuthErrors.Limited((int)Math.Ceiling(
            (await cache.GetTimeToLiveAsync(key, ct) ?? TimeSpan.FromMinutes(15)).TotalSeconds)) : null;
    }
    public async Task<UserOtp> IssueAsync(User user, OtpType type, CancellationToken ct)
    {
        // Retire earlier codes, including a code with the same timestamp.
        var previous = await db.UserOtps.Where(item => item.UserId == user.Id && item.OtpType == type && item.UsedAt == null).ToListAsync(ct);
        foreach (var item in previous) item.MarkUsed(clock.UtcNow);
        var code = otp.Generate();
        var record = new UserOtp(user.Id, type, otp.Hash(user.Id, type, code), clock.UtcNow);
        db.UserOtps.Add(record);
        afterCommit.Add(async token =>
        {
            try { await email.SendOtpAsync(user.Email, code, type, token); }
            catch (Exception exception)
            {
                // SMTP exception messages may contain server responses: never log their message/body.
                logger.LogError("OTP email delivery failed for user {UserId}; failure type {FailureType}.",
                    user.Id, exception.GetType().Name);
            }
        });
        return record;
    }
    public async Task<Error?> VerifyAsync(Guid userId, OtpType type, string code, CancellationToken ct)
    {
        var key = $"otp:attempts:{userId}:{type}";
        if (await cache.GetAsync<long>(key, ct) >= 5)
            return AuthErrors.Limited((int)Math.Ceiling((await cache.GetTimeToLiveAsync(key, ct) ?? TimeSpan.FromMinutes(5)).TotalSeconds));
        var record = await db.UserOtps.Where(item => item.UserId == userId && item.OtpType == type && item.UsedAt == null)
            .OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.ExpiresAt).FirstOrDefaultAsync(ct);
        // IssueAsync retires every earlier code; only the latest issuance can remain unused.
        if (record is null || record.UsedAt != null || record.ExpiresAt <= clock.UtcNow
            || !otp.Verify(userId, type, code, record.OtpHash))
        {
            var attempts = await cache.IncrementAsync(key, TimeSpan.FromMinutes(5), ct);
            return attempts >= 5 ? AuthErrors.Limited(300) : AuthErrors.InvalidOtp;
        }
        record.MarkUsed(clock.UtcNow);
        await cache.RemoveAsync(key, ct);
        return null;
    }
}

