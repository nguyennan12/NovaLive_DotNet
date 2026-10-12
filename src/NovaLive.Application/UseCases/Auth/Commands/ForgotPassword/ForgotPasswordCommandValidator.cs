using FluentValidation;

namespace NovaLive.Application.UseCases.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.Email).NotEmpty().MaximumLength(255).EmailAddress();
        });
    }
}
