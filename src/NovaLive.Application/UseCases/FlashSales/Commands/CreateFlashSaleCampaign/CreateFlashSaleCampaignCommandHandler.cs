using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;

namespace NovaLive.Application.UseCases.FlashSales.Commands.CreateFlashSaleCampaign;

public sealed class CreateFlashSaleCampaignCommandHandler(
    IFlashSaleRepository flashSaleRepository,
    ICacheService cacheService)
    : ICommandHandler<CreateFlashSaleCampaignCommand, FlashSaleCampaignResponse>
{
    public async Task<Result<FlashSaleCampaignResponse>> Handle(
        CreateFlashSaleCampaignCommand command,
        CancellationToken ct)
    {
        var req = command.Request;

        var campaign = new FlashSaleCampaign(
            name: req.Name,
            startAt: req.StartAt,
            endAt: req.EndAt,
            bannerUrl: req.BannerUrl);

        await flashSaleRepository.AddCampaignAsync(campaign, ct);
        await flashSaleRepository.SaveChangesAsync(ct);

        await cacheService.RemoveAsync("flashsales:active", ct);

        return campaign.ToResponse();
    }
}
