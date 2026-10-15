namespace NovaLive.Application.Common.Messaging;
// Handler prepares external I/O first, then explicitly delegates all writes to ITransactionManager.
public interface IManagesTransactionCommand : ITransactionalCommand;
