using FluentValidation;

namespace NovaLive.Application.UseCases.Auth.Commands.VerifyOtp;

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.UserId).NotEmpty();
        RuleFor(x => x.Data.OtpCode).NotEmpty().Matches("^[0-9]{6}$");
        });
    }
}
