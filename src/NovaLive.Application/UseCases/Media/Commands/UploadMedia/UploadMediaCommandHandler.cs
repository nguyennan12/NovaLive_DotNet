using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Media;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Media.Commands.UploadMedia;

/// <summary>
/// Command handler xử lý tải tập tin media lên MinIO S3 storage và trả về thông tin CDN URL.
/// </summary>
/// <param name="fileStorageService">Service giao tiếp với kho lưu trữ đối tượng MinIO S3.</param>
/// <param name="logger">Logger ghi vết quá trình tải tập tin.</param>
public sealed class UploadMediaCommandHandler(
    IFileStorageService fileStorageService,
    ILogger<UploadMediaCommandHandler> logger)
    : ICommandHandler<UploadMediaCommand, UploadMediaResponse>
{
    /// <summary>
    /// Xử lý lưu trữ tập tin media vào bucket chỉ định và tạo phản hồi chứa đường dẫn CDN.
    /// </summary>
    /// <param name="command">Command chứa luồng dữ liệu tập tin, tên file, content-type và bucket đích.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Kết quả chứa UploadMediaResponse hoặc mã lỗi tương ứng.</returns>
    public async Task<Result<UploadMediaResponse>> Handle(
        UploadMediaCommand command,
        CancellationToken cancellationToken)
    {
        var bucket = string.IsNullOrWhiteSpace(command.Bucket)
            ? StorageBuckets.Products
            : command.Bucket.Trim();

        logger.LogInformation(
            "Đang thực hiện upload file: {FileName}, dung lượng: {Length} bytes vào bucket: {Bucket}",
            command.FileName, command.Length, bucket);

        try
        {
            var cdnUrl = await fileStorageService.UploadAsync(
                command.FileStream,
                command.FileName,
                command.ContentType,
                bucket,
                cancellationToken);

            logger.LogInformation("Upload file thành công. CDN URL: {CdnUrl}", cdnUrl);

            return new UploadMediaResponse(
                Url: cdnUrl,
                FileName: Path.GetFileName(command.FileName),
                Bucket: bucket,
                Size: command.Length);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Tham số upload file không hợp lệ: {Message}", ex.Message);
            if (ex.Message.Contains("rỗng", StringComparison.OrdinalIgnoreCase) ||
                (ex.ParamName == "fileStream" && ex.Message.Contains("không hợp lệ", StringComparison.OrdinalIgnoreCase)))
            {
                return MediaErrors.Empty;
            }

            if (ex.Message.Contains("ảnh", StringComparison.OrdinalIgnoreCase) &&
                ex.Message.Contains("vượt quá", StringComparison.OrdinalIgnoreCase))
            {
                return MediaErrors.ImageExceedsLimit;
            }

            if (ex.Message.Contains("video", StringComparison.OrdinalIgnoreCase) &&
                ex.Message.Contains("vượt quá", StringComparison.OrdinalIgnoreCase))
            {
                return MediaErrors.VideoExceedsLimit;
            }

            return MediaErrors.InvalidExtension;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi upload file lên MinIO: {FileName}", command.FileName);
            return MediaErrors.UploadFailed();
        }
    }
}
