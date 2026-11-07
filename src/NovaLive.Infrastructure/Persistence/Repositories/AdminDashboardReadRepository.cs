using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Shops;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class AdminDashboardReadRepository(AppDbContext db) : IAdminDashboardReadRepository
{
    public async Task<AdminDashboardReadModel> GetAsync(DateTimeOffset? from, DateTimeOffset? to,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var paidOrders = db.ParentOrders.AsNoTracking()
            .Where(order => order.PaymentStatus == ParentOrderPaymentStatus.Paid);
        if (from is not null) paidOrders = paidOrders.Where(order => order.CreatedAt >= from);
        if (to is not null) paidOrders = paidOrders.Where(order => order.CreatedAt <= to);

        var orderSummary = await paidOrders
            .GroupBy(_ => 1)
            .Select(group => new
            {
                TotalGmv = group.Sum(order => order.GrandTotal),
                TotalOrdersCount = group.Count()
            })
            .SingleOrDefaultAsync(ct);

        var newUserRoleCountsQuery =
            from userRole in db.UserRoles.AsNoTracking()
            join user in db.Users.AsNoTracking().Where(user => user.DeletedAt == null)
                on userRole.UserId equals user.Id
            where userRole.RoleId == SystemRoleIds.Buyer || userRole.RoleId == SystemRoleIds.Seller
            select userRole;
        if (from is not null) newUserRoleCountsQuery = newUserRoleCountsQuery.Where(userRole => userRole.GrantedAt >= from);
        if (to is not null) newUserRoleCountsQuery = newUserRoleCountsQuery.Where(userRole => userRole.GrantedAt <= to);
        var newUserRoleCounts = await newUserRoleCountsQuery
            .GroupBy(userRole => userRole.RoleId)
            .Select(group => new { RoleId = group.Key, Count = group.Select(userRole => userRole.UserId).Distinct().Count() })
            .ToListAsync(ct);
        var newBuyerCount = newUserRoleCounts
            .Where(count => count.RoleId == SystemRoleIds.Buyer)
            .Select(count => count.Count)
            .SingleOrDefault();
        var newSellerCount = newUserRoleCounts
            .Where(count => count.RoleId == SystemRoleIds.Seller)
            .Select(count => count.Count)
            .SingleOrDefault();

        var totalActiveShopsCount = await db.Shops.AsNoTracking()
            .CountAsync(shop => shop.Status == ShopStatus.Active && shop.DeletedAt == null, ct);

        var paidSubOrders =
            from subOrder in db.SubOrders.AsNoTracking()
            join parentOrder in paidOrders on subOrder.ParentOrderId equals parentOrder.Id
            select new { subOrder.SubTotal, subOrder.SellerEarnings };
        var sellerEarningsSummary = await paidSubOrders
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Count = group.Count(),
                PopulatedCount = group.Count(subOrder => subOrder.SellerEarnings > 0m),
                CalculatedFee = group.Sum(subOrder => subOrder.SellerEarnings > 0m
                    ? subOrder.SubTotal - subOrder.SellerEarnings
                    : Math.Round(subOrder.SubTotal * 0.05m, 2))
            })
            .SingleOrDefaultAsync(ct);
        
        decimal? totalPlatformFeeRevenue = sellerEarningsSummary?.CalculatedFee ?? 0m;

        var deliveredSubOrders = db.SubOrders.AsNoTracking().Where(subOrder => subOrder.DeliveredAt != null);
        if (from is not null) deliveredSubOrders = deliveredSubOrders.Where(subOrder => subOrder.DeliveredAt >= from);
        if (to is not null) deliveredSubOrders = deliveredSubOrders.Where(subOrder => subOrder.DeliveredAt <= to);
        var returnsInPeriod = db.OrderReturns.AsNoTracking();
        if (from is not null) returnsInPeriod = returnsInPeriod.Where(orderReturn => orderReturn.CreatedAt >= from);
        if (to is not null) returnsInPeriod = returnsInPeriod.Where(orderReturn => orderReturn.CreatedAt <= to);
        var disputesInPeriod = returnsInPeriod
            .Where(orderReturn => orderReturn.Status == ReturnStatus.AdminDispute
                || orderReturn.Status == ReturnStatus.AdminApproved
                || orderReturn.Status == ReturnStatus.AdminRejected);
        var deliverySummary = await deliveredSubOrders
            .GroupBy(_ => 1)
            .Select(group => new
            {
                DeliveredCount = group.Count(),
                ReturnedCount = group.Count(subOrder => returnsInPeriod.Any(orderReturn => orderReturn.SubOrderId == subOrder.Id)),
                DisputedCount = group.Count(subOrder => disputesInPeriod.Any(orderReturn => orderReturn.SubOrderId == subOrder.Id))
            })
            .SingleOrDefaultAsync(ct);
        var deliveredCount = deliverySummary?.DeliveredCount ?? 0;
        var returnRatePercent = ToPercent(deliverySummary?.ReturnedCount ?? 0, deliveredCount);
        var disputeRatePercent = ToPercent(deliverySummary?.DisputedCount ?? 0, deliveredCount);

        var pendingPayouts = await db.SellerPayouts.AsNoTracking()
            .Where(payout => payout.Status == PayoutStatus.Pending)
            .OrderBy(payout => payout.ScheduledAt)
            .ThenBy(payout => payout.CreatedAt)
            .ThenBy(payout => payout.Id)
            .Select(payout => new PendingSellerPayoutReadModel(
                payout.Id,
                payout.ShopId,
                payout.Amount,
                payout.BankName,
                payout.BankAccount,
                payout.ScheduledAt))
            .ToListAsync(ct);

        var dailyOrderData = await paidOrders
            .Select(order => new { order.GrandTotal, order.CreatedAt })
            .ToListAsync(ct);

        var dailyMap = dailyOrderData
            .GroupBy(order => order.CreatedAt.UtcDateTime.ToString("yyyy-MM-dd"))
            .ToDictionary(group => group.Key, group => new
            {
                Revenue = group.Sum(order => order.GrandTotal),
                Count = group.Count()
            });

        var startDate = (from ?? now.AddDays(-29)).UtcDateTime.Date;
        var endDate = (to ?? now).UtcDateTime.Date;
        if (endDate < startDate) endDate = startDate;
        var totalDays = Math.Min(90, (endDate - startDate).Days + 1);

        var dailyRevenue = Enumerable.Range(0, totalDays)
            .Select(offset =>
            {
                var d = startDate.AddDays(offset).ToString("yyyy-MM-dd");
                return new DailyRevenuePointReadModel(
                    d,
                    dailyMap.TryGetValue(d, out var entry) ? entry.Revenue : 0m,
                    dailyMap.TryGetValue(d, out var entry2) ? entry2.Count : 0);
            })
            .ToList();

        // 12 months of current year
        var currentYear = (to ?? now).UtcDateTime.Year;
        var startOfYear = new DateTimeOffset(new DateTime(currentYear, 1, 1), TimeSpan.Zero);
        var endOfYear = new DateTimeOffset(new DateTime(currentYear, 12, 31, 23, 59, 59), TimeSpan.Zero);

        var yearlyPaidOrders = await db.ParentOrders.AsNoTracking()
            .Where(order => order.PaymentStatus == ParentOrderPaymentStatus.Paid
                && order.CreatedAt >= startOfYear
                && order.CreatedAt <= endOfYear)
            .Select(order => new { order.GrandTotal, order.CreatedAt })
            .ToListAsync(ct);

        var monthlyGroup = yearlyPaidOrders
            .GroupBy(order => order.CreatedAt.UtcDateTime.Month)
            .ToDictionary(group => group.Key, group => new
            {
                Revenue = group.Sum(order => order.GrandTotal),
                Count = group.Count()
            });

        var monthlyRevenue = Enumerable.Range(1, 12)
            .Select(m => new MonthlyRevenuePointReadModel(
                $"Tháng {m}",
                monthlyGroup.TryGetValue(m, out var val) ? val.Revenue : 0m,
                monthlyGroup.TryGetValue(m, out var val2) ? val2.Count : 0))
            .ToList();

        return new AdminDashboardReadModel(
            orderSummary?.TotalGmv ?? 0m,
            orderSummary?.TotalOrdersCount ?? 0,
            newBuyerCount,
            newSellerCount,
            totalActiveShopsCount,
            totalPlatformFeeRevenue,
            returnRatePercent,
            disputeRatePercent,
            pendingPayouts,
            dailyRevenue,
            monthlyRevenue);
    }

    private static decimal ToPercent(int numerator, int denominator) =>
        denominator == 0 ? 0m : Math.Round((decimal)numerator * 100m / denominator, 2);
}
