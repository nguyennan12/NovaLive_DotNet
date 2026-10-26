using FluentValidation;
namespace NovaLive.Application.UseCases.Returns.Queries.GetReturnDetail;
public sealed class GetReturnDetailQueryValidator : AbstractValidator<GetReturnDetailQuery>
{
    public GetReturnDetailQueryValidator() { RuleFor(q => q.Id).NotEmpty(); }
}
