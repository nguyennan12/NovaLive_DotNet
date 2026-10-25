using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetSellerLivestreams;

public sealed record GetSellerLivestreamsQuery(GetSellerLivestreamsRequest Request)
    : IQuery<PagedResult<SellerLivestreamResponse>>;
