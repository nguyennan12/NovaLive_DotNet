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
                PlatformFee = group.Sum(subOrder => subOrder.SubTotal - subOrder.SellerEarnings)
            })
            .SingleOrDefaultAsync(ct);
        // T32 currently does not populate SellerEarnings. Do not mistake the entity's
        // default zero for a calculated seller earning and report SubTotal as platform fee.
        decimal? totalPlatformFeeRevenue = sellerEarningsSummary is null
            ? 0m
            : sellerEarningsSummary.PopulatedCount == sellerEarningsSummary.Count
                ? sellerEarningsSummary.PlatformFee
                : null;

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

        return new AdminDashboardReadModel(
            orderSummary?.TotalGmv ?? 0m,
            orderSummary?.TotalOrdersCount ?? 0,
            newBuyerCount,
            newSellerCount,
            totalActiveShopsCount,
            totalPlatformFeeRevenue,
            returnRatePercent,
            disputeRatePercent,
            pendingPayouts);
    }

    private static decimal ToPercent(int numerator, int denominator) =>
        denominator == 0 ? 0m : Math.Round((decimal)numerator * 100m / denominator, 2);
}
