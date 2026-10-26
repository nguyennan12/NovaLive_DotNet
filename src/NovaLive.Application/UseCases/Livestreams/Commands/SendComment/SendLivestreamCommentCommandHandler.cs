using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.SendComment;

public sealed class SendLivestreamCommentCommandHandler(
    ILivestreamSessionRepository sessionRepository,
    ILivestreamCommentRepository commentRepository,
    ILivestreamCacheManager cacheManager,
    ILivestreamNotificationService notificationService,
    IDateTimeProvider dateTimeProvider,
    IAfterCommitActions afterCommit)
    : ICommandHandler<SendLivestreamCommentCommand, LivestreamCommentResponse>
{
    public async Task<Result<LivestreamCommentResponse>> Handle(
        SendLivestreamCommentCommand command,
        CancellationToken ct)
    {
        var session = await sessionRepository.GetByIdAsync(command.SessionId, ct);
        if (session is null)
        {
            return LivestreamErrors.NotFound;
        }

        if (session.Status != LivestreamStatus.Live)
        {
            return LivestreamErrors.NotLive;
        }

        var now = dateTimeProvider.UtcNow;
        var comment = new LivestreamComment(
            session.Id,
            command.UserId,
            command.DisplayName,
            command.Content,
            command.IsQuestion,
            now);

        await commentRepository.AddAsync(comment, ct);

        var response = comment.ToResponse();

        afterCommit.Add(async token =>
        {
            await cacheManager.AddCommentAsync(session.Id, response, token);
            await notificationService.BroadcastReceiveMessageAsync(session.Id, response, token);
        });

        return response;
    }
}
