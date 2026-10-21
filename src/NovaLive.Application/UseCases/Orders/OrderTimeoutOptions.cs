namespace NovaLive.Application.UseCases.Orders;

public sealed class OrderTimeoutOptions
{
    public const string SectionName = "OrderTimeout";
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan PaymentTimeout { get; set; } = TimeSpan.FromMinutes(15);
    public int BatchSize { get; set; } = 100;
}
