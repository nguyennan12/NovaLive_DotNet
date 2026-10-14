using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Services;

namespace NovaLive.Infrastructure.Storage;

public sealed class MinioBucketInitializer : IHostedService
{
    private readonly MinioFileStorageService _storageService;
    private readonly ILogger<MinioBucketInitializer> _logger;

    public MinioBucketInitializer(
        MinioFileStorageService storageService,
        ILogger<MinioBucketInitializer> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

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

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
