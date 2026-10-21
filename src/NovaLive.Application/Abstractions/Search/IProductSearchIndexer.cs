namespace NovaLive.Application.Abstractions.Search;

public interface IProductSearchIndexer
{
    /// <returns>The number of live SPUs whose search vector was rebuilt.</returns>
    Task<int> RebuildSearchVectorAsync(Guid spuId, CancellationToken cancellationToken);
}
