using FluentValidation;

namespace NovaLive.Application.UseCases.Wallets.Commands.RejectPayout;

public sealed class RejectPayoutCommandValidator : AbstractValidator<RejectPayoutCommand>
{
    public RejectPayoutCommandValidator()
    {
        RuleFor(x => x.PayoutId).NotEmpty();
        RuleFor(x => x.Request.Reason)
            .NotEmpty().WithMessage("Lý do từ chối không được để trống")
            .MaximumLength(500).WithMessage("Lý do từ chối không được vượt quá 500 ký tự");
    }
}
