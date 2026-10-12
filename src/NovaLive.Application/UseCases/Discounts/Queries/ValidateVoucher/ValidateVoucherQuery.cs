using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Discounts;

namespace NovaLive.Application.UseCases.Discounts.Queries.ValidateVoucher;

public record ValidateVoucherQuery(ValidateVoucherRequest Request)
    : IQuery<VoucherValidationResponse>;
