using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetSellerLivestreamAnalytics;

public sealed record GetSellerLivestreamAnalyticsQuery(GetSellerLivestreamAnalyticsRequest Request)
    : IQuery<SellerLivestreamAnalyticsResponse>;
