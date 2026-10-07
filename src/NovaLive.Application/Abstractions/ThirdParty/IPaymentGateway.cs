namespace NovaLive.Application.Abstractions.ThirdParty;

public interface IPaymentGateway
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(PaymentIntentRequest request, CancellationToken cancellationToken = default);
}

public sealed record PaymentIntentRequest(Guid PaymentId, decimal Amount, string Currency, string Description);

public sealed record PaymentIntentResult(string ProviderReference, string? PayUrl, string? QrCode);
