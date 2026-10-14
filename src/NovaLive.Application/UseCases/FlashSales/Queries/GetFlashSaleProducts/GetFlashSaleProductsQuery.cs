using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.FlashSales;

namespace NovaLive.Application.UseCases.FlashSales.Queries.GetFlashSaleProducts;

public record GetFlashSaleProductsQuery(Guid CampaignId, int Page = 1, int Size = 20)
    : IQuery<PagedResult<FlashSaleItemResponse>>;
