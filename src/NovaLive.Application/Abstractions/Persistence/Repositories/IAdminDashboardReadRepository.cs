namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IAdminDashboardReadRepository
{
    Task<AdminDashboardReadModel> GetAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct);
}

public sealed record AdminDashboardReadModel(
    decimal TotalGmv,
    int TotalOrdersCount,
    int NewBuyerCount,
    int NewSellerCount,
    int TotalActiveShopsCount,
    decimal? TotalPlatformFeeRevenue,
    decimal ReturnRatePercent,
    decimal DisputeRatePercent,
    IReadOnlyList<PendingSellerPayoutReadModel> PendingPayouts,
    IReadOnlyList<DailyRevenuePointReadModel>? DailyRevenue = null,
    IReadOnlyList<MonthlyRevenuePointReadModel>? MonthlyRevenue = null);

public sealed record PendingSellerPayoutReadModel(
    Guid Id,
    Guid ShopId,
    decimal Amount,
    string BankName,
    string BankAccount,
    DateTimeOffset ScheduledAt);
