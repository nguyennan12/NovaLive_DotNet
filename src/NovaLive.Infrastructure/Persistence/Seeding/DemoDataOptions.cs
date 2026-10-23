namespace NovaLive.Infrastructure.Persistence.Seeding;

public sealed class DemoDataOptions
{
    public const string SectionName = "DemoData";

    public string SellerEmail { get; set; } = "seller@novalive.local";

    public string SellerPassword { get; set; } = "Seller123!";
}
