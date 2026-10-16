using FluentValidation;

namespace NovaLive.Application.UseCases.Livestreams.Commands.CreatePublisherToken;

public sealed class CreateLivestreamPublisherTokenCommandValidator : AbstractValidator<CreateLivestreamPublisherTokenCommand>
{
    public CreateLivestreamPublisherTokenCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Mã phiên livestream không được để trống.");
    }
}
