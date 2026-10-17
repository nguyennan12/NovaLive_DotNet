namespace NovaLive.Api.Services;

public interface ILivestreamReactionBatcher
{
    void AddReaction(Guid sessionId, string reactionType);
}
