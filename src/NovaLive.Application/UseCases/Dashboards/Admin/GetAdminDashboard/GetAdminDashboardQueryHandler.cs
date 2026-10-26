using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Dashboards;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Dashboards.Admin.GetAdminDashboard;

public sealed class GetAdminDashboardQueryHandler(IAdminDashboardReadRepository dashboardReadRepository)
    : IQueryHandler<GetAdminDashboardQuery, AdminDashboardResponse>
{
    public async Task<Result<AdminDashboardResponse>> Handle(GetAdminDashboardQuery query, CancellationToken ct)
    {
        var data = await dashboardReadRepository.GetAsync(query.From, query.To, ct);
        var response = new AdminDashboardResponse(
            data.TotalGmv,
            data.TotalOrdersCount,
            data.NewBuyerCount,
            data.NewSellerCount,
            data.TotalActiveShopsCount,
            data.TotalPlatformFeeRevenue,
            data.ReturnRatePercent,
            data.DisputeRatePercent,
            data.PendingPayouts.Select(payout => new PendingSellerPayoutDto(
                payout.Id,
                payout.ShopId,
                payout.Amount,
                payout.BankName,
                payout.BankAccount,
                payout.ScheduledAt.UtcDateTime)).ToArray());

        return response;
    }
}
