using FluentValidation;
namespace NovaLive.Application.UseCases.Orders.Commands.CancelSubOrder;
public sealed class CancelSubOrderCommandValidator : AbstractValidator<CancelSubOrderCommand>
{
    public CancelSubOrderCommandValidator()
    {
        RuleFor(x => x.SubOrderId).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () => { RuleFor(x => x.Request.Reason).NotEmpty().MaximumLength(500); });
    }
}
