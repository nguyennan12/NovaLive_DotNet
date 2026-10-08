namespace NovaLive.Contracts.V1.Dashboards;

public record AdminDashboardMetricsDto(
    decimal TotalGmv,
    int TotalOrdersCount,
    int TotalActiveUsersCount,
    int TotalActiveShopsCount,
    decimal TotalPlatformFeeRevenue,
    decimal TotalDisputedAmount,
    List<DailyRevenuePointDto> RevenueChart);

public record SellerDashboardAnalyticsDto(
    decimal TotalSalesRevenue,
    int TotalOrdersCount,
    decimal WalletAvailableBalance,
    List<TopSellingSkuDto> TopSkus,
    List<LivestreamPerformanceSummaryDto> RecentLives);

public record DailyRevenuePointDto(
    DateTime Date,
    decimal Revenue,
    int OrdersCount);

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
    DateTime Date);
