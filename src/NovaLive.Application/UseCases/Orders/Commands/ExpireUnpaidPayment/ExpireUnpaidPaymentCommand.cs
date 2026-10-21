using NovaLive.Application.Common.Messaging;

namespace NovaLive.Application.UseCases.Orders.Commands.ExpireUnpaidPayment;

public sealed record ExpireUnpaidPaymentCommand(Guid PaymentId) : ICommand, ITransactionalCommand;
