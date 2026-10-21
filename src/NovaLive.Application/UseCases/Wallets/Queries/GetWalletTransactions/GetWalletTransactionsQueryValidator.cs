using FluentValidation;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Wallets.Queries.GetWalletTransactions;

public sealed class GetWalletTransactionsQueryValidator : AbstractValidator<GetWalletTransactionsQuery>
{
    public GetWalletTransactionsQueryValidator()
    {
        RuleFor(x => x.Request.Page).GreaterThan(0);
        RuleFor(x => x.Request.Size).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.Request.Type)
            .Must(type => string.IsNullOrWhiteSpace(type)
                || (Enum.TryParse<WalletTxType>(type, true, out var parsed) && Enum.IsDefined(parsed)))
            .WithMessage("Loại giao dịch ví không hợp lệ.");
        RuleFor(x => x.Request.To)
            .Must((query, to) => !query.Request.From.HasValue || !to.HasValue || query.Request.From <= to)
            .WithMessage("Thời gian bắt đầu phải nhỏ hơn hoặc bằng thời gian kết thúc.");
    }
}
