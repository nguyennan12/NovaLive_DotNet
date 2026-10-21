using FluentValidation;

namespace NovaLive.Application.UseCases.Wallets.Commands.CreatePayoutRequest;

public sealed class CreatePayoutRequestCommandValidator : AbstractValidator<CreatePayoutRequestCommand>
{
    public CreatePayoutRequestCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.Amount)
            .GreaterThanOrEqualTo(50_000m)
            .WithMessage("Số tiền rút tối thiểu là 50.000 VNĐ.");
    }
}
