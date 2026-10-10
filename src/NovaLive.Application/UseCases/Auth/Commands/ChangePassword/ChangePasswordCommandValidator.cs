using FluentValidation;
using NovaLive.Application.UseCases.Auth.Common;

namespace NovaLive.Application.UseCases.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.OldPassword).NotEmpty().Must(AuthValidation.BcryptLength).WithMessage("Password is too long.");
        RuleFor(x => x.Data.NewPassword).Must(AuthValidation.ValidPassword).WithMessage(AuthValidation.PasswordMessage);
        });
    }
}
