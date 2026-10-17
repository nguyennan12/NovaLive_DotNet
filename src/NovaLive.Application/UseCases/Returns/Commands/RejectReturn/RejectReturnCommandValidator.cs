using FluentValidation;
namespace NovaLive.Application.UseCases.Returns.Commands.RejectReturn;
public sealed class RejectReturnCommandValidator : AbstractValidator<RejectReturnCommand>
{
    public RejectReturnCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty(); RuleFor(c => c.Request).NotNull();
        When(c => c.Request != null, () =>
        {
            RuleFor(c => c.Request.Reason).NotEmpty().MaximumLength(2000);
            RuleFor(c => c.Request.EvidenceUrls).NotEmpty(); RuleForEach(c => c.Request.EvidenceUrls).Must(ReturnValidation.IsMediaUrl);
        });
    }
}
