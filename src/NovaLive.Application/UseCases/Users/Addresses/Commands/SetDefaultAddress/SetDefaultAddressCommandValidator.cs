using FluentValidation;

namespace NovaLive.Application.UseCases.Users.Addresses.Commands.SetDefaultAddress;

public sealed class SetDefaultAddressCommandValidator : AbstractValidator<SetDefaultAddressCommand>
{
    public SetDefaultAddressCommandValidator()
    {
        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("ID địa chỉ không hợp lệ.");
    }
}
