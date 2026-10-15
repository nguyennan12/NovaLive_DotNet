using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Services;

namespace NovaLive.Infrastructure.Storage;

/// <summary>
/// Hosted Service chạy khi ứng dụng khởi động để kiểm tra và tự động khởi tạo các bucket S3 trên MinIO.
/// </summary>
public sealed class MinioBucketInitializer : IHostedService
{
    private readonly MinioFileStorageService _storageService;
    private readonly ILogger<MinioBucketInitializer> _logger;

    /// <summary>
    /// Khởi tạo MinioBucketInitializer.
    /// </summary>
    /// <param name="storageService">Dịch vụ lưu trữ MinioFileStorageService.</param>
    /// <param name="logger">Logger ghi nhận tiến trình khởi tạo.</param>
    public MinioBucketInitializer(
        MinioFileStorageService storageService,
        ILogger<MinioBucketInitializer> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Thực hiện duyệt qua toàn bộ các bucket hệ thống (StorageBuckets.All), tạo mới nếu chưa có và áp dụng policy đọc công khai nếu là bucket public.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Bắt đầu tự động kiểm tra và khởi tạo các S3 Buckets...");

            foreach (var bucket in StorageBuckets.All)
            {
                await _storageService.EnsureBucketExistsAsync(bucket, cancellationToken);
            }

            _logger.LogInformation(
                "Đã khởi tạo xong các S3 Buckets: {Buckets} (Public: {PublicBuckets}, Private: {PrivateBuckets})",
                string.Join(", ", StorageBuckets.All),
                string.Join(", ", StorageBuckets.Public),
                string.Join(", ", StorageBuckets.Private));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra trong quá trình khởi tạo các S3 Buckets trên MinIO khi khởi chạy.");
        }
    }

    /// <summary>
    /// Xử lý khi ứng dụng dừng.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
