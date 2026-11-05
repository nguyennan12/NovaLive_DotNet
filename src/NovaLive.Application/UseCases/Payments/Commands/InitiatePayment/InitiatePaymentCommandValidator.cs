using FluentValidation;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Payments.Commands.InitiatePayment;

public sealed class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.ParentOrderId).NotEmpty();
            RuleFor(x => x.Request.PaymentMethod).Must(method => method is not null &&
                Enum.GetNames<PaymentMethod>().Contains(method, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Phương thức thanh toán phải là MoMo, VietQR hoặc COD.");
        });
    }
}
