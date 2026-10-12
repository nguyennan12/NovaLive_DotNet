using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Users;
using NovaLive.Application.UseCases.Auth.Common;
using NovaLive.Application.UseCases.Auth.Errors;

namespace NovaLive.Application.UseCases.Auth.Commands.Register;

public sealed class RegisterCommandHandler(IAppDbContext db, IAuthPersistence persistence, IPasswordHasher passwords,
    IDateTimeProvider clock, OtpFlowService otps) : ICommandHandler<RegisterCommand, RegisterUserResponse>
{
    public async Task<Result<RegisterUserResponse>> Handle(RegisterCommand request, CancellationToken ct)
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
