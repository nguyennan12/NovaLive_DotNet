using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Application.Abstractions.ThirdParty;

public interface IPaymentGatewayAdapter
{
    string GatewayName { get; }

    Task<PaymentIntentResult> CreatePaymentIntentAsync(PaymentIntentRequest request, CancellationToken cancellationToken = default);
}

public interface IMomoPaymentAdapter : IPaymentGatewayAdapter
{
    Task<MomoCreatePaymentResponse> CreatePaymentUrlAsync(
        string orderId,
        long amount,
        string orderInfo,
        string? extraData = null,
        CancellationToken ct = default);

    bool VerifySignature(MomoIpnRequest ipnRequest);
}

public interface IVietQrPaymentAdapter : IPaymentGatewayAdapter
{
    Task<VietQrResponse> GenerateDynamicQrAsync(string orderCode, decimal amount, string? description = null,
        CancellationToken cancellationToken = default);

    Task<VietQrTestCallbackResponse> TriggerTestCallbackAsync(VietQrResponse qr,
        CancellationToken cancellationToken = default);

    bool TryIssueCallbackToken(string username, string password, out VietQrTokenResponse? response);

    bool VerifyCallbackToken(string token);

    bool IsExpectedReceivingAccount(string? bankAccount);

    bool IsExpectedOrderId(string orderCode, string? providerOrderId);

    bool VerifyWebhookSignature(string rawPayload, string? signature);

    string? ExtractOrderCode(string content);
}
