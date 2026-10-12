namespace NovaLive.Infrastructure.Shipping.Ghn;

public sealed class GhnOptions
{
    public const string SectionName = "Ghn";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public int ShopId { get; set; }
    public string WebhookSecret { get; set; } = string.Empty;
    public int DefaultServiceTypeId { get; set; }
    public int TimeoutSeconds { get; set; }
}
