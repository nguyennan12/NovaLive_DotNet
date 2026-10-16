using FluentValidation;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetActiveLivestreams;

public sealed class GetActiveLivestreamsQueryValidator : AbstractValidator<GetActiveLivestreamsQuery>
{
    public GetActiveLivestreamsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100.");
    }
}
