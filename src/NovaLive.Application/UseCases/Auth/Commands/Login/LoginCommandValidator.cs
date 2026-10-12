using FluentValidation;
using NovaLive.Application.UseCases.Auth.Common;

namespace NovaLive.Application.UseCases.Auth.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.EmailOrPhone).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Data.Password).NotEmpty().Must(AuthValidation.BcryptLength).WithMessage("Password is too long.");
        });
    }
}
