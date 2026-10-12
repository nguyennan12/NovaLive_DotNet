using System.Text.Json;
using FluentValidation;

namespace NovaLive.Application.UseCases.Products.Commands.CreateSpu;

public sealed class CreateSpuCommandValidator : AbstractValidator<CreateSpuCommand>
{
    public CreateSpuCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Dữ liệu tạo sản phẩm không được để trống.");

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

            RuleFor(x => x.Request.Skus)
                .NotNull().WithMessage("Danh sách biến thể SKU không được để trống.")
                .Must(skus => skus != null && skus.Count > 0).WithMessage("Sản phẩm phải có ít nhất một biến thể SKU.");

            When(x => x.Request.Skus != null && x.Request.Skus.Count > 0, () =>
            {
                RuleFor(x => x.Request.Skus)
                    .Must(skus =>
                    {
                        var codes = skus.Select(s => s.SkuCode.Trim().ToUpperInvariant()).ToList();
                        return codes.Count == codes.Distinct().Count();
                    })
                    .WithMessage("Mã SKU trong cùng sản phẩm không được trùng lặp nhau.");

                RuleForEach(x => x.Request.Skus).ChildRules(sku =>
                {
                    sku.RuleFor(s => s.SkuCode)
                        .NotEmpty().WithMessage("Mã SKU không được để trống.")
                        .MaximumLength(100).WithMessage("Mã SKU không được vượt quá 100 ký tự.");

                    sku.RuleFor(s => s.AttributesJson)
                        .Must(BeValidJson)
                        .When(s => !string.IsNullOrWhiteSpace(s.AttributesJson))
                        .WithMessage("Thuộc tính biến thể (AttributesJson) phải là chuỗi JSON hợp lệ.");

                    sku.RuleFor(s => s.OriginalPrice)
                        .GreaterThanOrEqualTo(0).WithMessage("Giá gốc của SKU phải lớn hơn hoặc bằng 0.");

                    sku.RuleFor(s => s.SellPrice)
                        .GreaterThanOrEqualTo(0).WithMessage("Giá bán của SKU phải lớn hơn hoặc bằng 0.");

                    sku.RuleFor(s => s.WeightGram)
                        .GreaterThan(0).WithMessage("Trọng lượng SKU phải lớn hơn 0 gram.");

                    sku.RuleFor(s => s.InitialStock)
                        .GreaterThanOrEqualTo(0).WithMessage("Tồn kho ban đầu của SKU phải lớn hơn hoặc bằng 0.");
                });
            });
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
