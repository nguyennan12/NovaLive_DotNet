using FluentValidation;
namespace NovaLive.Application.UseCases.Orders.Commands.ConfirmSubOrder;
public sealed class ConfirmSubOrderCommandValidator : AbstractValidator<ConfirmSubOrderCommand>
{
    public ConfirmSubOrderCommandValidator()
    {
        RuleFor(x => x.SubOrderId).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () => { RuleFor(x => x.Request.WarehouseAddressId).NotEmpty(); });
    }
}
