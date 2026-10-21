using FluentValidation;

namespace NovaLive.Application.UseCases.Wallets.Commands.ApprovePayout;

public sealed class ApprovePayoutCommandValidator : AbstractValidator<ApprovePayoutCommand>
{
    public ApprovePayoutCommandValidator()
    {
        RuleFor(x => x.PayoutId).NotEmpty();
        RuleFor(x => x.Request.TransferRef)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Request.TransferRef));
    }
}
