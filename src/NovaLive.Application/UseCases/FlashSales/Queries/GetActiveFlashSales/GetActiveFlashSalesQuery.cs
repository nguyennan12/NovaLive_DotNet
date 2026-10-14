using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.FlashSales;

namespace NovaLive.Application.UseCases.FlashSales.Queries.GetActiveFlashSales;

public record GetActiveFlashSalesQuery : IQuery<IReadOnlyList<FlashSaleCampaignResponse>>;
