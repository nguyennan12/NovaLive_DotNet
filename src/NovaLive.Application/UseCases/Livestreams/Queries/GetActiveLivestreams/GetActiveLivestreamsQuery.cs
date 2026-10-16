using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetActiveLivestreams;

public record GetActiveLivestreamsQuery(int Page = 1, int Size = 20)
    : IQuery<PagedResult<LivestreamSessionResponse>>;
