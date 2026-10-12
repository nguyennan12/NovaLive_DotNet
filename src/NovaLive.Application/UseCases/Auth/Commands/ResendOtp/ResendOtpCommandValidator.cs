using FluentValidation;

namespace NovaLive.Application.UseCases.Auth.Commands.ResendOtp;

public sealed class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.EmailOrPhone).NotEmpty().MaximumLength(255);
        });
    }
}
