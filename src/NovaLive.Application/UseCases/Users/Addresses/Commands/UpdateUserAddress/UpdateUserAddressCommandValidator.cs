using FluentValidation;

namespace NovaLive.Application.UseCases.Users.Addresses.Commands.UpdateUserAddress;

public sealed class UpdateUserAddressCommandValidator : AbstractValidator<UpdateUserAddressCommand>
{
    public UpdateUserAddressCommandValidator()
    {
        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("ID địa chỉ không hợp lệ.");

        RuleFor(x => x.Request.RecipientName)
            .NotEmpty().WithMessage("Tên người nhận không được để trống.")
            .MaximumLength(150).WithMessage("Tên người nhận tối đa 150 ký tự.");

        RuleFor(x => x.Request.Phone)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.")
            .MaximumLength(20).WithMessage("Số điện thoại tối đa 20 ký tự.")
            .Matches(@"^[0-9+\-\s()]*$").WithMessage("Số điện thoại không đúng định dạng.");

        RuleFor(x => x.Request.ProvinceName)
            .NotEmpty().WithMessage("Tỉnh/Thành phố không được để trống.")
            .MaximumLength(100).WithMessage("Tên Tỉnh/Thành phố tối đa 100 ký tự.");

        RuleFor(x => x.Request.DistrictName)
            .NotEmpty().WithMessage("Quận/Huyện không được để trống.")
            .MaximumLength(100).WithMessage("Tên Quận/Huyện tối đa 100 ký tự.");

        RuleFor(x => x.Request.WardName)
            .NotEmpty().WithMessage("Phường/Xã không được để trống.")
            .MaximumLength(100).WithMessage("Tên Phường/Xã tối đa 100 ký tự.");

        RuleFor(x => x.Request.DetailAddress)
            .NotEmpty().WithMessage("Địa chỉ chi tiết không được để trống.")
            .MaximumLength(300).WithMessage("Địa chỉ chi tiết tối đa 300 ký tự.");

        RuleFor(x => x.Request.WardCode)
            .MaximumLength(20).WithMessage("Mã Phường/Xã tối đa 20 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.WardCode));
    }
}
