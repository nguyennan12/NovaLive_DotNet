using FluentValidation;

namespace NovaLive.Application.UseCases.Shipping.Commands.Webhook;

public sealed class HandleShippingWebhookCommandValidator : AbstractValidator<HandleShippingWebhookCommand>
{
    public HandleShippingWebhookCommandValidator()
    {
        RuleFor(x => x.Provider).IsInEnum();
        RuleFor(x => x.TrackingCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Status).IsInEnum();
    }
}
