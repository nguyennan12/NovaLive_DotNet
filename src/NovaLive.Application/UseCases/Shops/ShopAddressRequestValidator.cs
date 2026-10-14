using FluentValidation;
using NovaLive.Contracts.V1.Shops;
namespace NovaLive.Application.UseCases.Shops;

public sealed class ShopAddressRequestValidator : AbstractValidator<CreateShopAddressRequest>
{
    public ShopAddressRequestValidator()
    {
        RuleFor(x => x.WarehouseName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(20).Matches(ShopValidation.PhonePattern);
        RuleFor(x => x.ProvinceId).NotNull().GreaterThan(0);
        RuleFor(x => x.DistrictId).NotNull().GreaterThan(0);
        RuleFor(x => x.WardCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ProvinceName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DistrictName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WardName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DetailAddress).NotEmpty().MaximumLength(300);
    }
}
