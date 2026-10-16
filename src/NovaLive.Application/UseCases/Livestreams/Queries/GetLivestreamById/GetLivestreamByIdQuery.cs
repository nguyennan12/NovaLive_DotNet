using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetLivestreamById;

public record GetLivestreamByIdQuery(Guid SessionId)
    : IQuery<LivestreamSessionDetailResponse>;
