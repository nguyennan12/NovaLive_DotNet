using FluentValidation;

namespace NovaLive.Application.UseCases.Orders.Commands.ExpireUnpaidPayment;

public sealed class ExpireUnpaidPaymentCommandValidator : AbstractValidator<ExpireUnpaidPaymentCommand>
{
    public ExpireUnpaidPaymentCommandValidator() => RuleFor(c => c.PaymentId).NotEmpty();
}
