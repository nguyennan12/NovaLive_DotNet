using FluentValidation;
using Microsoft.Extensions.Options;
namespace NovaLive.Application.UseCases.Shops.Commands.UpdateShopInfo;

public sealed class UpdateShopInfoCommandValidator : AbstractValidator<UpdateShopInfoCommand>
{
    public UpdateShopInfoCommandValidator(IOptions<KycUrlOptions> options)
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.ShopName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Request.Description).NotNull();
            RuleFor(x => x.Request.Phone).NotEmpty().MaximumLength(20).Matches(ShopValidation.PhonePattern);
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(255);
            RuleFor(x => x.Request.LogoUrl).MaximumLength(500)
                .Must(url => url is null || options.Value.IsAllowedCdnUrl(url));
            RuleFor(x => x.Request.BannerUrl).MaximumLength(500)
                .Must(url => url is null || options.Value.IsAllowedCdnUrl(url));
        });
    }
}
