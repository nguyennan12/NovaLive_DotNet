using MediatR;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork, ITransactionManager transactions)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IBaseCommand)
        {
            return await next(cancellationToken);
        }

        if (request is ITransactionalCommand)
            return await transactions.ExecuteAsync(() => next(cancellationToken),
                request is ICommitOnFailureCommand, cancellationToken);

        var response = await next(cancellationToken);

        if (response is Result { IsFailure: true })
        {
            return response;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
