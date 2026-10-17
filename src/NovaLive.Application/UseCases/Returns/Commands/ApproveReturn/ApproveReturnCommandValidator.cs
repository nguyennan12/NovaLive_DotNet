using FluentValidation;
namespace NovaLive.Application.UseCases.Returns.Commands.ApproveReturn;
public sealed class ApproveReturnCommandValidator : AbstractValidator<ApproveReturnCommand>
{
    public ApproveReturnCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty(); RuleFor(c => c.Request).NotNull();
        When(c => c.Request != null, () => RuleFor(c => c.Request.ReasonNote).MaximumLength(2000));
    }
}
