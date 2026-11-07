using FluentValidation;

namespace NovaLive.Application.UseCases.Shipping.Commands.SimulateShipmentDelivered;

public sealed class SimulateShipmentDeliveredCommandValidator : AbstractValidator<SimulateShipmentDeliveredCommand>
{
    public SimulateShipmentDeliveredCommandValidator() => RuleFor(x => x.SubOrderId).NotEmpty();
}
