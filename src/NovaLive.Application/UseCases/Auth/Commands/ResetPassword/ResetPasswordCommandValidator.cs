using FluentValidation;
using NovaLive.Application.UseCases.Auth.Common;

namespace NovaLive.Application.UseCases.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.Email).NotEmpty().MaximumLength(255).EmailAddress();
        RuleFor(x => x.Data.OtpCode).NotEmpty().Matches("^[0-9]{6}$");
        RuleFor(x => x.Data.NewPassword).Must(AuthValidation.ValidPassword).WithMessage(AuthValidation.PasswordMessage);
        });
    }
}
