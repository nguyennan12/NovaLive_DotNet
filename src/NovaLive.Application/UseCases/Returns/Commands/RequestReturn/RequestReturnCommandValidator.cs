using FluentValidation;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Returns.Commands.RequestReturn;
public sealed class RequestReturnCommandValidator : AbstractValidator<RequestReturnCommand>
{
    public RequestReturnCommandValidator()
    {
        RuleFor(c => c.Request).NotNull();
        When(c => c.Request != null, () =>
        {
            RuleFor(c => c.Request.SubOrderId).NotEmpty();
            RuleFor(c => c.Request.Reason).Must(r => Enum.GetNames<ReturnReason>().Contains(r));
            RuleFor(c => c.Request.Items).NotEmpty().Must(items => items == null || items.All(i => i != null)
                && items.Select(i => i.OrderItemId).Distinct().Count() == items.Count);
            RuleForEach(c => c.Request.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.OrderItemId).NotEmpty(); item.RuleFor(i => i.Quantity).GreaterThan(0);
                item.RuleFor(i => i.ReasonDetail).MaximumLength(2000);
            });
            RuleFor(c => c.Request.EvidenceUrls).NotEmpty();
            RuleForEach(c => c.Request.EvidenceUrls).Must(ReturnValidation.IsMediaUrl);
        });
    }
}
