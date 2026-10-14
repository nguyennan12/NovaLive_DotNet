using FluentValidation;

namespace NovaLive.Application.UseCases.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống.")
            .MaximumLength(200).WithMessage("Tên danh mục không được vượt quá 200 ký tự.");

        RuleFor(x => x.Request.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị phải lớn hơn hoặc bằng 0.");

        When(x => !string.IsNullOrEmpty(x.Request.IconUrl), () =>
        {
            RuleFor(x => x.Request.IconUrl)
                .MaximumLength(500).WithMessage("Đường dẫn icon không được vượt quá 500 ký tự.");
        });

        When(x => !string.IsNullOrEmpty(x.Request.Slug), () =>
        {
            RuleFor(x => x.Request.Slug)
                .MaximumLength(200).WithMessage("Slug không được vượt quá 200 ký tự.");
        });
    }
}
