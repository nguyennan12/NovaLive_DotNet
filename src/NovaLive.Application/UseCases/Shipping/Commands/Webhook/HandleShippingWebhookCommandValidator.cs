using FluentValidation;

namespace NovaLive.Application.UseCases.Shipping.Commands.Webhook;

public sealed class HandleShippingWebhookCommandValidator : AbstractValidator<HandleShippingWebhookCommand>
{
    public HandleShippingWebhookCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Dữ liệu webhook không được để trống.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.OrderCode)
                .NotEmpty().WithMessage("Mã đơn vận chuyển (OrderCode) không được để trống.");

            RuleFor(x => x.Request.Status)
                .NotEmpty().WithMessage("Trạng thái vận chuyển (Status) không được để trống.");
        });
    }
}
