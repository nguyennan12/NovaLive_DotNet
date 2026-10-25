using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Contracts.V1.Dashboards;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Dashboards.Seller.GetSellerDashboard;

public sealed class GetSellerDashboardQueryHandler(
    ISellerDashboardReadRepository dashboardReadRepository,
    ShopAccess shopAccess)
    : IQueryHandler<GetSellerDashboardQuery, SellerDashboardResponse>
{
    public async Task<Result<SellerDashboardResponse>> Handle(GetSellerDashboardQuery query, CancellationToken ct)
    {
        var owned = await shopAccess.GetSellerOwnedAsync(false, ct);
        if (owned.IsFailure)
        {
            return owned.Error;
        }

        var shop = owned.Value!;
        var data = await dashboardReadRepository.GetAsync(shop.Id, query.From, query.To, ct);

        var dailyRevenue = data.DailyRevenue
            .Select(d => new DailyRevenuePointDto(d.Date, d.Revenue, d.OrdersCount))
            .ToArray();

        var orderStatuses = data.OrderStatuses
            .Select(s => new OrderStatusBreakdownDto(s.Status, s.Count))
            .ToArray();

        var topSkus = data.TopSkus
            .Select(s => new TopSellingSkuDto(s.SkuId, s.SpuName, s.SkuCode, s.SoldQuantity, s.TotalRevenue))
            .ToArray();

        var recentLives = data.RecentLives
            .Select(l => new LivestreamPerformanceSummaryDto(l.SessionId, l.Title, l.PeakViewers, l.TotalOrdersCount, l.TotalRevenue, l.Date))
            .ToArray();

        var actions = new SellerActionSummaryDto(
            data.Actions.PendingOrders,
            data.Actions.ExpiringReturns,
            data.Actions.LowStockSkus);

        WalletOverviewDto? walletOverview = null;
        if (data.WalletOverview is not null)
        {
            var txs = data.WalletOverview.RecentTransactions
                .Select(t => new WalletTransactionSummaryDto(
                    t.Id,
                    t.Type,
                    t.Amount,
                    t.BalanceAfter,
                    t.Description,
                    t.CreatedAt))
                .ToArray();

            walletOverview = new WalletOverviewDto(
                data.WalletOverview.Balance,
                data.WalletOverview.LockedBalance,
                data.WalletOverview.Currency,
                txs);
        }

        var response = new SellerDashboardResponse(
            data.TotalSalesRevenue,
            data.TotalOrdersCount,
            data.WalletAvailableBalance,
            data.PendingOrdersCount,
            dailyRevenue,
            orderStatuses,
            topSkus,
            recentLives,
            actions,
            walletOverview);

        return response;
    }
}
