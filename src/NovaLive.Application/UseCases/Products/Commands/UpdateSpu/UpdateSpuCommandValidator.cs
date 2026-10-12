using System.Text.Json;
using FluentValidation;

namespace NovaLive.Application.UseCases.Products.Commands.UpdateSpu;

public sealed class UpdateSpuCommandValidator : AbstractValidator<UpdateSpuCommand>
{
    public UpdateSpuCommandValidator()
    {
        RuleFor(x => x.SpuId)
            .NotEmpty().WithMessage("ID sản phẩm không được để trống.");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Dữ liệu cập nhật sản phẩm không được để trống.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Tên sản phẩm không được để trống.")
                .MinimumLength(5).WithMessage("Tên sản phẩm phải có ít nhất 5 ký tự.")
                .MaximumLength(300).WithMessage("Tên sản phẩm không được vượt quá 300 ký tự.");

            RuleFor(x => x.Request.CategoryId)
                .NotEmpty().WithMessage("Danh mục sản phẩm không được để trống.");

            RuleFor(x => x.Request.ThumbnailUrl)
                .NotEmpty().WithMessage("Ảnh đại diện sản phẩm không được để trống.")
                .MaximumLength(500).WithMessage("Đường dẫn ảnh đại diện không được vượt quá 500 ký tự.");

            RuleFor(x => x.Request.AttributesConfigJson)
                .Must(BeValidJson)
                .When(x => !string.IsNullOrWhiteSpace(x.Request.AttributesConfigJson))
                .WithMessage("Cấu hình trục biến thể (AttributesConfigJson) phải là chuỗi JSON hợp lệ.");
        });
    }

    private static bool BeValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
