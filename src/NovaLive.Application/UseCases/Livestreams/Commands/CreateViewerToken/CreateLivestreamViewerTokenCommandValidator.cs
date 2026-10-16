using FluentValidation;

namespace NovaLive.Application.UseCases.Livestreams.Commands.CreateViewerToken;

public sealed class CreateLivestreamViewerTokenCommandValidator : AbstractValidator<CreateLivestreamViewerTokenCommand>
{
    public CreateLivestreamViewerTokenCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Mã phiên livestream không được để trống.");
    }
}
