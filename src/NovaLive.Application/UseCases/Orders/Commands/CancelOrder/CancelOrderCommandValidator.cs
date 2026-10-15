using FluentValidation;
namespace NovaLive.Application.UseCases.Orders.Commands.CancelOrder;
public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () => { RuleFor(x => x.Request.Reason).NotEmpty().MaximumLength(500); });
    }
}
