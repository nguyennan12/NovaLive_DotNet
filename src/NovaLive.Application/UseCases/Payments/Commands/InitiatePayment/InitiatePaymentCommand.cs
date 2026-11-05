using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Application.UseCases.Payments.Commands.InitiatePayment;

public sealed record InitiatePaymentCommand(InitiatePaymentRequest Request) : ICommand<PaymentInitResponse>;
