namespace NovaLive.Infrastructure.Payments.Momo;

public sealed class MomoOptions
{
    public const string SectionName = "Momo";

    public string PartnerCode { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string Endpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";

    public string RedirectUrl { get; set; } = "https://novalive.vn/orders/payment-result";

    public string IpnUrl { get; set; } = "https://api.novalive.vn/api/v1/payments/webhook/momo";

    public string RequestType { get; set; } = "captureWallet";
}
