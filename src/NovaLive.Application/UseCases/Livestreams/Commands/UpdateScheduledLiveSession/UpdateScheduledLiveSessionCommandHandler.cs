using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Livestreams;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Livestreams.Commands.UpdateScheduledLiveSession;

public sealed class UpdateScheduledLiveSessionCommandHandler(
    ILivestreamSessionRepository sessions,
    IAppDbContext db,
    ICurrentUser currentUser,
    ICurrentShop currentShop,
    IDateTimeProvider clock)
    : ICommandHandler<UpdateScheduledLiveSessionCommand>
{
    public async Task<Result> Handle(UpdateScheduledLiveSessionCommand command, CancellationToken ct)
    {
        var shopId = currentShop.ShopId ?? currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        var session = await sessions.GetByIdAndShopAsync(command.SessionId, shopId.Value, ct);
        if (session is null)
        {
            return LivestreamErrors.NotFound;
        }

        var request = command.Request;
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length < 3)
        {
            return LivestreamErrors.InvalidSchedule;
        }

        var update = session.UpdateSchedule(request.Title, request.ThumbnailUrl, request.ScheduledAt, clock.UtcNow);
        if (update.IsFailure)
        {
            return LivestreamErrors.InvalidSchedule;
        }

        sessions.Update(session);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
