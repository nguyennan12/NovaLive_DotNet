namespace NovaLive.Contracts.V1.Dashboards;

public record AdminDashboardResponse(
    decimal TotalGmv,
    int TotalOrdersCount,
    int NewBuyerCount,
    int NewSellerCount,
    int TotalActiveShopsCount,
    decimal? TotalPlatformFeeRevenue,
    decimal ReturnRatePercent,
    decimal DisputeRatePercent,
    IReadOnlyList<PendingSellerPayoutDto> PendingPayouts);

public record PendingSellerPayoutDto(
    Guid Id,
    Guid ShopId,
    decimal Amount,
    string BankName,
    string BankAccount,
    DateTime ScheduledAt);

public record SellerDashboardResponse(
    decimal TotalSalesRevenue,
    int TotalOrdersCount,
    decimal WalletAvailableBalance,
    int PendingOrdersCount,
    IReadOnlyList<DailyRevenuePointDto> DailyRevenue,
    IReadOnlyList<OrderStatusBreakdownDto> OrderStatuses,
    IReadOnlyList<TopSellingSkuDto> TopSkus,
    IReadOnlyList<LivestreamPerformanceSummaryDto> RecentLives,
    SellerActionSummaryDto Actions,
    WalletOverviewDto? WalletOverview = null);

public record SellerDashboardAnalyticsDto(
    decimal TotalSalesRevenue,
    int TotalOrdersCount,
    decimal WalletAvailableBalance,
    List<TopSellingSkuDto> TopSkus,
    List<LivestreamPerformanceSummaryDto> RecentLives);

public record DailyRevenuePointDto(
    string Date,
    decimal Revenue,
    int OrdersCount);

public record OrderStatusBreakdownDto(
    string Status,
    int Count);

public record TopSellingSkuDto(
    Guid SkuId,
    string SpuName,
    string SkuCode,
    int SoldQuantity,
    decimal TotalRevenue);

public record LivestreamPerformanceSummaryDto(
    Guid SessionId,
    string Title,
    int PeakViewers,
    int TotalOrdersCount,
    decimal TotalRevenue,
    string Date);

public record SellerActionSummaryDto(
    int PendingOrders,
    int ExpiringReturns,
    int LowStockSkus);

public record WalletOverviewDto(
    decimal Balance,
    decimal LockedBalance,
    string Currency,
    IReadOnlyList<WalletTransactionSummaryDto> RecentTransactions);

public record WalletTransactionSummaryDto(
    Guid Id,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    string? Description,
    DateTime CreatedAt);
