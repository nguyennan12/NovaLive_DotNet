using FluentValidation;

namespace NovaLive.Application.UseCases.Wallets.Commands.ApprovePayout;

public sealed class ApprovePayoutCommandValidator : AbstractValidator<ApprovePayoutCommand>
{
    public ApprovePayoutCommandValidator()
    {
        RuleFor(x => x.PayoutId).NotEmpty();
        RuleFor(x => x.Request.TransferRef)
            .NotEmpty()
            .MaximumLength(100)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Cần nhập mã giao dịch ngân hàng sau khi đã chuyển khoản.");
    }
}
