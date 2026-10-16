using FluentValidation;

namespace NovaLive.Application.UseCases.Livestreams.Commands.UnpinProduct;

public sealed class UnpinProductCommandValidator : AbstractValidator<UnpinProductCommand>
{
    public UnpinProductCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Mã phiên livestream không được để trống.");
    }
}
