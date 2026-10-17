using FluentValidation;
namespace NovaLive.Application.UseCases.Returns.Queries.GetSellerReturns;
public sealed class GetSellerReturnsQueryValidator : AbstractValidator<GetSellerReturnsQuery>
{
    public GetSellerReturnsQueryValidator()
    { RuleFor(q => q.Status).Must(ReturnValidation.IsStatus); RuleFor(q => q.Page).InclusiveBetween(1, 1000000); RuleFor(q => q.Size).InclusiveBetween(1, 100); }
}
