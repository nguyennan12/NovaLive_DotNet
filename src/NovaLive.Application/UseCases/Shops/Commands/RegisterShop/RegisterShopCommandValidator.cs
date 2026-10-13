using FluentValidation;
using Microsoft.Extensions.Options;
using NovaLive.Application.Common.Helpers;

namespace NovaLive.Application.UseCases.Shops.Commands.RegisterShop;

public sealed class RegisterShopCommandValidator : AbstractValidator<RegisterShopCommand>
{
    public RegisterShopCommandValidator(IOptions<KycUrlOptions> options)
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.ShopName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Request.Description).NotNull();
            RuleFor(x => x.Request.Slug).MaximumLength(200);
            RuleFor(x => x.Request).Must(r => SlugHelper.GenerateSlug(r.Slug ?? r.ShopName).Length is > 0 and <= 200)
                .WithMessage("Slug phải chứa chữ cái hoặc chữ số.");
            RuleFor(x => x.Request.Phone).NotEmpty().MaximumLength(20).Matches(ShopValidation.PhonePattern);
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(255);
            RuleFor(x => x.Request.TaxCode).MaximumLength(30);
            RuleFor(x => x.Request.BankAccount).NotEmpty().MaximumLength(30);
            RuleFor(x => x.Request.BankName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Request.BankBranch).MaximumLength(200);
            RuleFor(x => x.Request.KycFrontUrl).NotEmpty().MaximumLength(500).Must(options.Value.IsAllowed)
                .WithMessage("URL CCCD phải thuộc bucket kyc-docs của hệ thống.");
            RuleFor(x => x.Request.KycBackUrl).NotEmpty().MaximumLength(500).Must(options.Value.IsAllowed)
                .WithMessage("URL CCCD phải thuộc bucket kyc-docs của hệ thống.");
            RuleFor(x => x.Request.BusinessLicense).MaximumLength(500).Must(options.Value.IsAllowed)
                .When(x => x.Request.BusinessLicense is not null)
                .WithMessage("URL giấy phép phải thuộc bucket kyc-docs của hệ thống.");
        });
    }
}
