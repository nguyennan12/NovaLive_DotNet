using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Application.UseCases.Auth.Common;
using NovaLive.Application.UseCases.Auth.Errors;

namespace NovaLive.Application.UseCases.Auth.Commands.Logout;

public sealed class LogoutCommandHandler(IAuthPersistence persistence, ICurrentUser currentUser,
    IDateTimeProvider clock, AuthSessionService sessions) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is not Guid userId) return AuthErrors.InvalidCredentials;
        await persistence.LockUserAsync(userId, ct);
        await persistence.RevokeAllAsync(userId, clock.UtcNow, ct);
        await sessions.BlacklistCurrentAsync(ct);
        return Result.Success();
    }
}
