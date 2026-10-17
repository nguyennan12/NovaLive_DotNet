using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.SendComment;

public record SendLivestreamCommentCommand(
    Guid SessionId,
    Guid? UserId,
    string DisplayName,
    string Content,
    bool IsQuestion = false)
    : ICommand<LivestreamCommentResponse>, ITransactionalCommand;
