using FluentValidation;
namespace NovaLive.Application.UseCases.Returns.Queries.GetMyReturns;
public sealed class GetMyReturnsQueryValidator : AbstractValidator<GetMyReturnsQuery>
{
    public GetMyReturnsQueryValidator()
    { RuleFor(q => q.Status).Must(ReturnValidation.IsStatus); RuleFor(q => q.Page).GreaterThan(0); RuleFor(q => q.Size).InclusiveBetween(1, 100); }
}
