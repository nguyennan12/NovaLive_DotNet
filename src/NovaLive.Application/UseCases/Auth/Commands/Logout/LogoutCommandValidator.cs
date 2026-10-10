using FluentValidation;

namespace NovaLive.Application.UseCases.Auth.Commands.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.RefreshToken).NotEmpty().MaximumLength(512);
        });
    }
}
