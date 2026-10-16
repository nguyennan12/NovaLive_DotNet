using FluentValidation;

namespace NovaLive.Application.UseCases.Livestreams.Commands.EndLiveSession;

public sealed class EndLiveSessionCommandValidator : AbstractValidator<EndLiveSessionCommand>
{
    public EndLiveSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Mã phiên livestream không được để trống.");
    }
}
