using FluentValidation;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetLivestreamById;

public sealed class GetLivestreamByIdQueryValidator : AbstractValidator<GetLivestreamByIdQuery>
{
    public GetLivestreamByIdQueryValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Mã phiên livestream không được để trống.");
    }
}
