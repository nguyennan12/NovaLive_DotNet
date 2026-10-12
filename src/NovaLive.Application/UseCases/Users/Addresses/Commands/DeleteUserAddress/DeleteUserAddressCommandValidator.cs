using FluentValidation;

namespace NovaLive.Application.UseCases.Users.Addresses.Commands.DeleteUserAddress;

public sealed class DeleteUserAddressCommandValidator : AbstractValidator<DeleteUserAddressCommand>
{
    public DeleteUserAddressCommandValidator()
    {
        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("ID địa chỉ không hợp lệ.");
    }
}
