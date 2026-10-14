using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Media;

public static class MediaErrors
{
    public static readonly Error Empty =
        Error.Validation("Media.Empty", "File tải lên không được để trống.");

    public static readonly Error InvalidExtension =
        Error.Validation("Media.InvalidExtension", "Định dạng file không được hỗ trợ. Chỉ chấp nhận các định dạng: .jpg, .jpeg, .png, .webp, .mp4.");

    public static readonly Error ImageExceedsLimit =
        Error.Validation("Media.ImageExceedsLimit", "Dung lượng ảnh vượt quá giới hạn cho phép (tối đa 10MB).");

    public static readonly Error VideoExceedsLimit =
        Error.Validation("Media.VideoExceedsLimit", "Dung lượng video vượt quá giới hạn cho phép (tối đa 100MB).");

    public static readonly Error InvalidBucket =
        Error.Validation("Media.InvalidBucket", "Bucket lưu trữ không hợp lệ.");

    public static Error UploadFailed(string? message = null) =>
        Error.Failure("Media.UploadFailed", message ?? "Không thể tải file lên hệ thống lưu trữ MinIO.");
}
