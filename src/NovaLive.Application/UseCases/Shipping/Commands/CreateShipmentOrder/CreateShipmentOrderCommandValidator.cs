using FluentValidation;

namespace NovaLive.Application.UseCases.Shipping.Commands.CreateShipmentOrder;

public sealed class CreateShipmentOrderCommandValidator : AbstractValidator<CreateShipmentOrderCommand>
{
    public CreateShipmentOrderCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.SubOrderId).NotEmpty();
            RuleFor(x => x.Request.PickupAddressId).NotEmpty();
            RuleFor(x => x.Request.Provider).Must(p => p is "GHN" or "GHTK" or "ViettelPost");
            RuleFor(x => x.Request.ServiceCode).NotEmpty().MaximumLength(50);
            When(x => x.Request.Provider == "GHN", () =>
                RuleFor(x => x.Request.ServiceCode).Must(s => int.TryParse(s, out var id) && id > 0)
                    .WithMessage("Mã dịch vụ GHN phải là số nguyên dương."));
        });
    }
}
