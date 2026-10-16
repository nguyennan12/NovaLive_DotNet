using FluentValidation;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetPinnedProduct;

public sealed class GetPinnedProductQueryValidator : AbstractValidator<GetPinnedProductQuery>
{
    public GetPinnedProductQueryValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Mã phiên livestream không được để trống.");
    }
}
