using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Shops;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class SellerDashboardReadRepository(AppDbContext db) : ISellerDashboardReadRepository
{
    public async Task<SellerDashboardReadModel> GetAsync(
        Guid shopId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        // 1. Wallet & Transactions
        var wallet = await db.ShopWallets.AsNoTracking()
            .FirstOrDefaultAsync(w => w.ShopId == shopId, ct);

        decimal netWalletRevenue = 0m;
        List<WalletTransactionSummaryReadModel> recentTransactions = [];

        if (wallet is not null)
        {
            var txQuery = db.ShopWalletTransactions.AsNoTracking()
                .Where(tx => tx.WalletId == wallet.Id);

            if (from is not null) txQuery = txQuery.Where(tx => tx.CreatedAt >= from);
            if (to is not null) txQuery = txQuery.Where(tx => tx.CreatedAt <= to);

            var revenueSum = await txQuery
                .Where(tx => tx.Type == WalletTxType.OrderRevenue)
                .SumAsync(tx => (decimal?)tx.Amount, ct) ?? 0m;

            var refundSum = await txQuery
                .Where(tx => tx.Type == WalletTxType.OrderRefund)
                .SumAsync(tx => (decimal?)tx.Amount, ct) ?? 0m;

            netWalletRevenue = Math.Max(0m, revenueSum - refundSum);

            var rawTxs = await db.ShopWalletTransactions.AsNoTracking()
                .Where(tx => tx.WalletId == wallet.Id)
                .OrderByDescending(tx => tx.CreatedAt)
                .Take(10)
                .Select(tx => new
                {
                    tx.Id,
                    tx.Type,
                    tx.Amount,
                    tx.BalanceAfter,
                    tx.Description,
                    tx.CreatedAt
                })
                .ToListAsync(ct);

            recentTransactions = rawTxs
                .Select(tx => new WalletTransactionSummaryReadModel(
                    tx.Id,
                    tx.Type.ToString(),
                    tx.Amount,
                    tx.BalanceAfter,
                    tx.Description,
                    tx.CreatedAt.UtcDateTime))
                .ToList();
        }

        // 2. Shop SubOrders in Period
        var baseOrdersQuery = db.SubOrders.AsNoTracking()
            .Where(s => s.ShopId == shopId);

        var filteredOrdersQuery = baseOrdersQuery;
        if (from is not null) filteredOrdersQuery = filteredOrdersQuery.Where(s => s.CreatedAt >= from);
        if (to is not null) filteredOrdersQuery = filteredOrdersQuery.Where(s => s.CreatedAt <= to);

        var totalOrdersCount = await filteredOrdersQuery.CountAsync(ct);

        // Fallback revenue calculation if no wallet transactions yet
        var paidSubOrders = filteredOrdersQuery
            .Where(s => s.Status == SubOrderStatus.Delivered || s.Status == SubOrderStatus.Completed);

        var orderRevenue = await paidSubOrders
            .SumAsync(s => (decimal?)(s.SubTotal - s.ShopDiscountAmount), ct) ?? 0m;

        var totalSalesRevenue = netWalletRevenue > 0m ? netWalletRevenue : orderRevenue;

        // 3. Pending Orders Count (Confirmed needing warehouse assignment)
        var pendingOrdersCount = await baseOrdersQuery
            .CountAsync(s => s.Status == SubOrderStatus.Confirmed && s.WarehouseAddressId == null, ct);

        // 4. Daily Revenue Points
        var eligibleForDailyRevenue = await filteredOrdersQuery
            .Where(s => s.Status != SubOrderStatus.Cancelled && s.Status != SubOrderStatus.PendingPayment)
            .Select(s => new
            {
                s.CreatedAt,
                Revenue = s.SubTotal - s.ShopDiscountAmount
            })
            .ToListAsync(ct);

        var dailyRevenue = eligibleForDailyRevenue
            .GroupBy(s => s.CreatedAt.UtcDateTime.ToString("yyyy-MM-dd"))
            .Select(g => new DailyRevenuePointReadModel(
                g.Key,
                g.Sum(x => Math.Max(0m, x.Revenue)),
                g.Count()))
            .OrderBy(d => d.Date)
            .ToList();

        // 5. Order Statuses Breakdown
        var statusCounts = await filteredOrdersQuery
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var statusMap = statusCounts.ToDictionary(x => x.Status, x => x.Count);
        var allSubOrderStatuses = new[]
        {
            SubOrderStatus.PendingPayment,
            SubOrderStatus.Confirmed,
            SubOrderStatus.Processing,
            SubOrderStatus.Shipping,
            SubOrderStatus.Delivered,
            SubOrderStatus.Completed,
            SubOrderStatus.ReturnRequested,
            SubOrderStatus.Returned,
            SubOrderStatus.Refunded,
            SubOrderStatus.Cancelled
        };

        var orderStatuses = allSubOrderStatuses
            .Select(st => new OrderStatusBreakdownReadModel(st.ToString(), statusMap.GetValueOrDefault(st, 0)))
            .ToList();

        // 6. Top 10 Selling SKUs
        var topSkuAggregates = await (
            from item in db.OrderItems.AsNoTracking()
            join subOrder in filteredOrdersQuery on item.SubOrderId equals subOrder.Id
            where subOrder.Status != SubOrderStatus.Cancelled
            group item by item.SkuId into g
            select new
            {
                SkuId = g.Key,
                SoldQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ThenByDescending(x => x.SoldQuantity)
            .Take(10)
            .ToListAsync(ct);

        var skuIds = topSkuAggregates.Select(x => x.SkuId).Distinct().ToList();

        var skuDetails = await (
            from sku in db.Skus.AsNoTracking()
            join spu in db.Spus.AsNoTracking() on sku.SpuId equals spu.Id
            where skuIds.Contains(sku.Id)
            select new
            {
                SkuId = sku.Id,
                SkuCode = sku.SkuCode,
                SpuName = spu.Name
            })
            .ToDictionaryAsync(x => x.SkuId, ct);

        var topSkus = topSkuAggregates
            .Select(agg =>
            {
                skuDetails.TryGetValue(agg.SkuId, out var details);
                return new TopSellingSkuReadModel(
                    agg.SkuId,
                    details?.SpuName ?? "Sản phẩm",
                    details?.SkuCode ?? string.Empty,
                    agg.SoldQuantity,
                    agg.TotalRevenue);
            })
            .ToList();

        // 7. Recent Livestreams Performance
        var livesQuery = db.LivestreamSessions.AsNoTracking()
            .Where(l => l.ShopId == shopId);

        if (from is not null) livesQuery = livesQuery.Where(l => l.CreatedAt >= from || l.StartedAt >= from);
        if (to is not null) livesQuery = livesQuery.Where(l => l.CreatedAt <= to || l.StartedAt <= to);

        var recentLiveSessions = await livesQuery
            .OrderByDescending(l => l.CreatedAt)
            .Take(10)
            .Select(l => new
            {
                l.Id,
                l.Title,
                l.PeakViewerCount,
                l.StartedAt,
                l.CreatedAt
            })
            .ToListAsync(ct);

        var liveSessionIds = recentLiveSessions.Select(l => l.Id).ToList();

        var liveProductStats = await db.LivestreamProducts.AsNoTracking()
            .Where(p => liveSessionIds.Contains(p.SessionId))
            .GroupBy(p => p.SessionId)
            .Select(g => new
            {
                SessionId = g.Key,
                TotalOrdersCount = g.Sum(p => p.SoldInLive),
                TotalRevenue = g.Sum(p => p.SoldInLive * (p.FlashPrice ?? 0m))
            })
            .ToDictionaryAsync(x => x.SessionId, ct);

        var recentLives = recentLiveSessions
            .Select(l =>
            {
                liveProductStats.TryGetValue(l.Id, out var stats);
                var sessionDate = (l.StartedAt ?? l.CreatedAt).UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                return new LivestreamPerformanceSummaryReadModel(
                    l.Id,
                    l.Title,
                    l.PeakViewerCount,
                    stats?.TotalOrdersCount ?? 0,
                    stats?.TotalRevenue ?? 0m,
                    sessionDate);
            })
            .ToList();

        // 8. Actions (Cần xử lý ngay)
        var twoDaysAgo = now.AddHours(-48);
        var expiringReturns = await (
            from ret in db.OrderReturns.AsNoTracking()
            join sub in db.SubOrders.AsNoTracking() on ret.SubOrderId equals sub.Id
            where sub.ShopId == shopId
                && ret.Status == ReturnStatus.Pending
                && ret.CreatedAt <= twoDaysAgo
            select ret.Id)
            .CountAsync(ct);

        var lowStockSkus = await db.Inventories.AsNoTracking()
            .Where(inv => inv.ShopId == shopId && (inv.QtyOnHand - inv.ReservedQty) <= inv.MinStock)
            .CountAsync(ct);

        var actions = new SellerActionSummaryReadModel(
            pendingOrdersCount,
            expiringReturns,
            lowStockSkus);

        // 9. Wallet Overview
        var walletOverview = wallet is null
            ? new WalletOverviewReadModel(0m, 0m, "VND", [])
            : new WalletOverviewReadModel(wallet.Balance, wallet.LockedBalance, wallet.Currency, recentTransactions);

        return new SellerDashboardReadModel(
            totalSalesRevenue,
            totalOrdersCount,
            wallet?.Balance ?? 0m,
            pendingOrdersCount,
            dailyRevenue,
            orderStatuses,
            topSkus,
            recentLives,
            actions,
            walletOverview);
    }
}
