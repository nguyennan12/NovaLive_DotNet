using FluentValidation;

namespace NovaLive.Application.UseCases.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.RefreshToken).NotEmpty().MaximumLength(512);
        });
    }
}
