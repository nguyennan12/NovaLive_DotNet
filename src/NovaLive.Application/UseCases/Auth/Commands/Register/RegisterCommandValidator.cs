using FluentValidation;
using NovaLive.Application.UseCases.Auth.Common;

namespace NovaLive.Application.UseCases.Auth.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.Email).NotEmpty().MaximumLength(255).EmailAddress();
        RuleFor(x => x.Data.Phone).NotEmpty().Matches(@"^(0|\+84)[35789][0-9]{8}$");
        RuleFor(x => x.Data.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Data.Password).Must(AuthValidation.ValidPassword).WithMessage(AuthValidation.PasswordMessage);
        });
    }
}
