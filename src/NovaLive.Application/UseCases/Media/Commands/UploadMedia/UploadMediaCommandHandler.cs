using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Media;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Media.Commands.UploadMedia;

public sealed class UploadMediaCommandHandler(
    IFileStorageService fileStorageService,
    ILogger<UploadMediaCommandHandler> logger)
    : ICommandHandler<UploadMediaCommand, UploadMediaResponse>
{
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
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Tham số upload file không hợp lệ: {Message}", ex.Message);
            return MediaErrors.InvalidExtension;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi upload file lên MinIO: {FileName}", command.FileName);
            return MediaErrors.UploadFailed(ex.Message);
        }
    }
}
