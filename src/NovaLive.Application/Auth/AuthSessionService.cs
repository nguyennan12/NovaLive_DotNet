using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Users;
namespace NovaLive.Application.Auth;

public sealed class AuthSessionService(IAppDbContext db, IJwtTokenService jwt, IRefreshTokenService refresh,
    IDateTimeProvider clock, ICacheService cache, ICurrentUser currentUser)
{
    public static string Normalize(string value) => value.Trim().ToLowerInvariant();
    public static string NormalizePhone(string value)
    {
        var phone = value.Trim();
        return phone.StartsWith("+84", StringComparison.Ordinal) ? "0" + phone[3..] : phone;
    }
    public async Task<User?> FindUserAsync(string identifier, CancellationToken ct)
    {
        var normalized = Normalize(identifier);
        var phone = NormalizePhone(identifier);
        return await db.Users.SingleOrDefaultAsync(user => user.DeletedAt == null
            && (user.Email == normalized || user.Phone == phone), ct);
    }
    public async Task<(List<string> Roles, Guid? ShopId)> GetContextAsync(Guid userId, CancellationToken ct)
    {
        var roles = await (from userRole in db.UserRoles join role in db.Roles on userRole.RoleId equals role.Id
                           where userRole.UserId == userId select role.Name).Distinct().ToListAsync(ct);
        var shopId = await db.Shops.Where(shop => shop.OwnerId == userId && shop.Status == ShopStatus.Active && shop.DeletedAt == null)
            .Select(shop => (Guid?)shop.Id).FirstOrDefaultAsync(ct);
        return (roles, shopId);
    }
    public async Task<AuthResponse> IssueAsync(User user, string? ip, string? device, CancellationToken ct)
    {
        var (roles, shopId) = await GetContextAsync(user.Id, ct);
        var access = jwt.Create(user.Id, user.Email, roles, shopId);
        var token = refresh.Create();
        db.RefreshTokens.Add(new RefreshToken(user.Id, token.Hash, device, ip, clock.UtcNow));
        return new(access.Value, token.Value, access.ExpiresAt.UtcDateTime,
            new(user.Id, user.Email, user.Phone ?? "", user.FullName, user.AvatarUrl, user.AccountStatus.ToString(), shopId, roles));
    }
    public async Task ResetLoginAsync(string email, CancellationToken ct)
    {
        foreach (var prefix in new[] { "login:failed:", "login:lock:", "login:lockcount:" })
            await cache.RemoveAsync(prefix + email, ct);
    }
    public async Task BlacklistCurrentAsync(CancellationToken ct)
    {
        var ttl = currentUser.AccessTokenExpiresAt - clock.UtcNow;
        if (!string.IsNullOrWhiteSpace(currentUser.Jti) && ttl > TimeSpan.Zero)
            await cache.SetAsync("Blacklist:" + currentUser.Jti, true, ttl, ct);
    }
}

