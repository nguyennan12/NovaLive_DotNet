using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Application.UseCases.Payments.Queries.GetPaymentStatus;

public sealed record GetPaymentStatusQuery(Guid PaymentId) : IQuery<PaymentStatusResponse>;
