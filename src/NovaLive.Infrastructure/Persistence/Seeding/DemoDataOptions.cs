namespace NovaLive.Infrastructure.Persistence.Seeding;

public sealed class DemoDataOptions
{
    public const string SectionName = "DemoData";

    public bool Enabled { get; set; }

    public string SellerEmail { get; set; } = string.Empty;

    public string SellerPassword { get; set; } = string.Empty;
}
