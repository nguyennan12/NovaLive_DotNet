using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Livestreams;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Livestreams.Commands.CancelScheduledLiveSession;

public sealed class CancelScheduledLiveSessionCommandHandler(
    ILivestreamSessionRepository sessionRepository,
    IAppDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<CancelScheduledLiveSessionCommand>
{
    public async Task<Result> Handle(CancelScheduledLiveSessionCommand command, CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        var session = await sessionRepository.GetByIdAndShopAsync(command.SessionId, shopId.Value, ct);
        if (session is null)
        {
            return LivestreamErrors.NotFound;
        }

        var result = session.CancelScheduled(DateTimeOffset.UtcNow);
        if (result.IsFailure)
        {
            return result.Error;
        }

        sessionRepository.Update(session);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
