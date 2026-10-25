namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface ISellerDashboardReadRepository
{
    Task<SellerDashboardReadModel> GetAsync(Guid shopId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct);
}

public sealed record SellerDashboardReadModel(
    decimal TotalSalesRevenue,
    int TotalOrdersCount,
    decimal WalletAvailableBalance,
    int PendingOrdersCount,
    IReadOnlyList<DailyRevenuePointReadModel> DailyRevenue,
    IReadOnlyList<OrderStatusBreakdownReadModel> OrderStatuses,
    IReadOnlyList<TopSellingSkuReadModel> TopSkus,
    IReadOnlyList<LivestreamPerformanceSummaryReadModel> RecentLives,
    SellerActionSummaryReadModel Actions,
    WalletOverviewReadModel? WalletOverview);

public sealed record DailyRevenuePointReadModel(
    string Date,
    decimal Revenue,
    int OrdersCount);

public sealed record OrderStatusBreakdownReadModel(
    string Status,
    int Count);

public sealed record TopSellingSkuReadModel(
    Guid SkuId,
    string SpuName,
    string SkuCode,
    int SoldQuantity,
    decimal TotalRevenue);

public sealed record LivestreamPerformanceSummaryReadModel(
    Guid SessionId,
    string Title,
    int PeakViewers,
    int TotalOrdersCount,
    decimal TotalRevenue,
    string Date);

public sealed record SellerActionSummaryReadModel(
    int PendingOrders,
    int ExpiringReturns,
    int LowStockSkus);

public sealed record WalletOverviewReadModel(
    decimal Balance,
    decimal LockedBalance,
    string Currency,
    IReadOnlyList<WalletTransactionSummaryReadModel> RecentTransactions);

public sealed record WalletTransactionSummaryReadModel(
    Guid Id,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    string? Description,
    DateTime CreatedAt);
