namespace NovaLive.Infrastructure.Shipping.Ghn;

public sealed class GhnOptions
{
    public const string SectionName = "Ghn";

    public string BaseUrl { get; set; } = "https://dev-online-gateway.ghn.vn";
    public string ApiToken { get; set; } = "ff432d05-3989-11f1-a973-aee5264794df";
    public int ShopId { get; set; }
    public string WebhookSecret { get; set; } = "ff432d05-3989-11f1-a973-aee5264794df";
    public int DefaultServiceTypeId { get; set; } = 2;
    public int TimeoutSeconds { get; set; } = 15;
}
