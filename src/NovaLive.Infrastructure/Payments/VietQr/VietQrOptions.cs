namespace NovaLive.Infrastructure.Payments.VietQr;

public sealed class VietQrOptions
{
    public const string SectionName = "VietQr";

    public string BankBin { get; set; } = "970422";

    public string AccountNumber { get; set; } = "0000123456789";

    public string AccountName { get; set; } = "CONG TY NOVALIVE";

    public string ClientId { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string WebhookSecret { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.vietqr.vn";
}
