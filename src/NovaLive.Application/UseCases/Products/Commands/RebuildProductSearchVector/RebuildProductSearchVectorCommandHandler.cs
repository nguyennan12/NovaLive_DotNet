using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Search;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Products.Commands.RebuildProductSearchVector;

public sealed class RebuildProductSearchVectorCommandHandler(
    IProductSearchIndexer indexer,
    ILogger<RebuildProductSearchVectorCommandHandler> logger) : ICommandHandler<RebuildProductSearchVectorCommand>
{
    public async Task<Result> Handle(RebuildProductSearchVectorCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var affectedRows = await indexer.RebuildSearchVectorAsync(command.SpuId, cancellationToken);
            if (affectedRows == 0)
            {
                logger.LogInformation("Skipping search vector rebuild for missing or soft-deleted SPU {SpuId}", command.SpuId);
            }

            return Result.Success();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to rebuild search vector for SPU {SpuId}", command.SpuId);
            return ProductErrors.SearchIndexRebuildFailed;
        }
    }
}
