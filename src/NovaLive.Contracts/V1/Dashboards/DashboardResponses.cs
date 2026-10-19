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
