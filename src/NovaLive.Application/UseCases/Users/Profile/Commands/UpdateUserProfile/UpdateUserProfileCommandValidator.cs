using FluentValidation;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Users.Profile.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.Request.FullName)
            .NotEmpty().WithMessage("Họ và tên không được để trống.")
            .MaximumLength(200).WithMessage("Họ và tên tối đa 200 ký tự.");

        RuleFor(x => x.Request.Phone)
            .MaximumLength(20).WithMessage("Số điện thoại tối đa 20 ký tự.")
            .Matches(@"^[0-9+\-\s()]*$").WithMessage("Số điện thoại không đúng định dạng.")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Phone));

        RuleFor(x => x.Request.Birthday)
            .Must(birthday => !birthday.HasValue || birthday.Value <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Ngày sinh không được ở tương lai.");

        RuleFor(x => x.Request.Gender)
            .Must(gender => string.IsNullOrWhiteSpace(gender) || Enum.TryParse<UserGender>(gender, true, out _))
            .WithMessage("Giới tính không hợp lệ. Chỉ chấp nhận Male, Female, Other, Unspecified.");

        RuleFor(x => x.Request.AvatarUrl)
            .MaximumLength(500).WithMessage("Đường dẫn ảnh đại diện tối đa 500 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.AvatarUrl));
    }
}
