using FluentValidation;
using NovaLive.Application.Abstractions.Services;

namespace NovaLive.Application.UseCases.Media.Commands.UploadMedia;

public class UploadMediaCommandValidator : AbstractValidator<UploadMediaCommand>
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".mp4"
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxImageSize = 10 * 1024 * 1024;   // 10MB
    private const long MaxVideoSize = 100 * 1024 * 1024;  // 100MB

    public UploadMediaCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("Tên file không được để trống.");

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("File tải lên không có nội dung.");

        RuleFor(x => x.Length)
            .LessThanOrEqualTo(MaxVideoSize)
            .WithMessage("Dung lượng file vượt quá giới hạn tối đa cho phép (100MB).");

        RuleFor(x => x.FileName)
            .Must(fileName =>
            {
                if (string.IsNullOrWhiteSpace(fileName)) return false;
                var ext = Path.GetExtension(fileName);
                return !string.IsNullOrEmpty(ext) && AllowedExtensions.Contains(ext);
            })
            .WithMessage("Định dạng file không được hỗ trợ. Chỉ chấp nhận các định dạng: .jpg, .jpeg, .png, .webp, .mp4.");

        When(x => !string.IsNullOrEmpty(x.FileName) && ImageExtensions.Contains(Path.GetExtension(x.FileName)), () =>
        {
            RuleFor(x => x.Length)
                .LessThanOrEqualTo(MaxImageSize)
                .WithMessage("Dung lượng ảnh vượt quá giới hạn cho phép (tối đa 10MB).");
        });

        When(x => !string.IsNullOrEmpty(x.FileName) && Path.GetExtension(x.FileName).Equals(".mp4", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Length)
                .LessThanOrEqualTo(MaxVideoSize)
                .WithMessage("Dung lượng video vượt quá giới hạn cho phép (tối đa 100MB).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Bucket), () =>
        {
            RuleFor(x => x.Bucket!)
                .Must(StorageBuckets.IsPublic)
                .WithMessage($"Bucket không hợp lệ. Chỉ chấp nhận các bucket công khai: {string.Join(", ", StorageBuckets.Public)}.");
        });
    }
}
