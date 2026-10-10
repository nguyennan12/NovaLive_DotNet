namespace NovaLive.Application.Common.Messaging;

// Security operations (refresh reuse detection) must persist revocations even when returning 401.
public interface ICommitOnFailureCommand : ITransactionalCommand;
