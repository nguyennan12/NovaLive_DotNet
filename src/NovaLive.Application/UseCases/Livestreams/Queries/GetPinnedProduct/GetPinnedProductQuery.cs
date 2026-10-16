using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetPinnedProduct;

public record GetPinnedProductQuery(Guid SessionId)
    : IQuery<PinnedProductResponse>;
