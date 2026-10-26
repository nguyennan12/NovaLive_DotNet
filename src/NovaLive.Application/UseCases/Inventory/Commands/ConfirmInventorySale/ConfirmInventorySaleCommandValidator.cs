using FluentValidation;

namespace NovaLive.Application.UseCases.Inventory.Commands.ConfirmInventorySale;

public sealed class ConfirmInventorySaleCommandValidator : AbstractValidator<ConfirmInventorySaleCommand>
{
    public ConfirmInventorySaleCommandValidator() => RuleFor(c => c.PaymentId).NotEmpty();
}
