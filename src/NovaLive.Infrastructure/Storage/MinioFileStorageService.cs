using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using NovaLive.Application.Abstractions.Services;

namespace NovaLive.Infrastructure.Storage;

/// <summary>
/// Dịch vụ lưu trữ tập tin sử dụng MinIO S3 Object Storage tương thích AWS S3.
/// </summary>
public sealed class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioOptions _options;
    private readonly ILogger<MinioFileStorageService> _logger;
    private readonly ConcurrentDictionary<string, bool> _initializedBuckets = new(StringComparer.OrdinalIgnoreCase);

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

    /// <summary>
    /// Khởi tạo MinioFileStorageService với MinIO client và options.
    /// </summary>
    public MinioFileStorageService(
        IMinioClient minioClient,
        IOptions<MinioOptions> options,
        ILogger<MinioFileStorageService> logger)
    {
        _minioClient = minioClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Upload file stream lên MinIO bucket và trả về CDN URL đầy đủ.
    /// </summary>
    /// <param name="fileStream">Stream dữ liệu file cần upload.</param>
    /// <param name="fileName">Tên file gốc bao gồm phần mở rộng.</param>
    /// <param name="contentType">MIME type của file.</param>
    /// <param name="bucketName">Tên bucket đích (mặc định: products).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>URL CDN đầy đủ để truy cập file.</returns>
    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string bucketName = StorageBuckets.Products,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            bucketName = StorageBuckets.Products;
        }

        // 1. Kiểm tra định dạng đuôi file (.jpg, .jpeg, .png, .webp, .mp4)
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                $"Định dạng file '{extension}' không được hỗ trợ. Chỉ chấp nhận các định dạng: .jpg, .jpeg, .png, .webp, .mp4.",
                nameof(fileName));
        }

        // 2. Kiểm tra dung lượng stream (ảnh <= 10MB, video <= 100MB)
        Stream uploadStream = fileStream;
        MemoryStream? bufferStream = null;
        long streamLength;

        try
        {
            if (fileStream.CanSeek)
            {
                streamLength = fileStream.Length;
                if (fileStream.Position != 0)
                {
                    fileStream.Position = 0;
                }
            }
            else
            {
                bufferStream = new MemoryStream();
                await fileStream.CopyToAsync(bufferStream, cancellationToken);
                bufferStream.Position = 0;
                uploadStream = bufferStream;
                streamLength = bufferStream.Length;
            }

            if (streamLength <= 0)
            {
                throw new ArgumentException("Dung lượng file không hợp lệ hoặc file rỗng.", nameof(fileStream));
            }

            if (ImageExtensions.Contains(extension) && streamLength > MaxImageSize)
            {
                throw new ArgumentException(
                    $"Dung lượng ảnh ({streamLength / (1024 * 1024.0):F2}MB) vượt quá giới hạn cho phép (tối đa 10MB).",
                    nameof(fileStream));
            }

            if (extension == ".mp4" && streamLength > MaxVideoSize)
            {
                throw new ArgumentException(
                    $"Dung lượng video ({streamLength / (1024 * 1024.0):F2}MB) vượt quá giới hạn cho phép (tối đa 100MB).",
                    nameof(fileStream));
            }

            // 3. Đảm bảo bucket đã tồn tại (nếu chưa thì khởi tạo tự động)
            await EnsureBucketExistsAsync(bucketName, cancellationToken);

            // 4. Sinh UUID ngẫu nhiên gắn vào tên file tránh ghi đè
            var baseFileName = Path.GetFileNameWithoutExtension(fileName);
            var sanitizedBaseName = string.Join("_", baseFileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
            if (string.IsNullOrEmpty(sanitizedBaseName))
            {
                sanitizedBaseName = "media";
            }
            var uniqueFileName = $"{Guid.NewGuid():N}_{sanitizedBaseName}{extension}";

            // 5. Chuẩn hóa Content-Type
            var resolvedContentType = string.IsNullOrWhiteSpace(contentType)
                ? GetDefaultContentType(extension)
                : contentType;

            // 6. Upload lên MinIO qua S3 SDK
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(uniqueFileName)
                .WithStreamData(uploadStream)
                .WithObjectSize(streamLength)
                .WithContentType(resolvedContentType);

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            _logger.LogInformation("Upload thành công lên S3 MinIO: {Bucket}/{ObjectName} ({Size} bytes)", bucketName, uniqueFileName, streamLength);

            // 7. Trả về URL CDN đầy đủ https://cdn.novalive.vn/{bucket}/{filename}
            var cdnBaseUrl = _options.CdnBaseUrl.TrimEnd('/');
            return $"{cdnBaseUrl}/{bucketName}/{uniqueFileName}";
        }
        finally
        {
            bufferStream?.Dispose();
        }
    }

    /// <summary>
    /// Upload file stream với tên file xác định lên bucket chỉ định.
    /// </summary>
    public async Task<string> UploadAsync(
        Stream content,
        string objectName,
        string contentType,
        CancellationToken cancellationToken)
    {
        return await UploadAsync(content, objectName, contentType, StorageBuckets.Products, cancellationToken);
    }

    /// <summary>
    /// Kiểm tra bucket đã tồn tại hay chưa, nếu chưa thì tạo mới và áp dụng chính sách đọc công khai nếu là public bucket.
    /// Luôn đảm bảo chính sách public-read được cập nhật cho cả bucket mới và bucket đã tồn tại.
    /// </summary>
    /// <param name="bucketName">Tên bucket cần kiểm tra.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        if (_initializedBuckets.ContainsKey(bucketName))
        {
            return;
        }

        try
        {
            var beArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool exists = await _minioClient.BucketExistsAsync(beArgs, cancellationToken);
            if (!exists)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(mbArgs, cancellationToken);
                _logger.LogInformation("Khởi tạo mới S3 Bucket: {Bucket}", bucketName);
            }

            // Áp dụng public-read policy cho cả bucket mới tạo lẫn bucket đã tồn tại từ trước
            if (StorageBuckets.IsPublic(bucketName))
            {
                await SetPublicReadPolicyAsync(bucketName, cancellationToken);
            }

            _initializedBuckets.TryAdd(bucketName, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi kiểm tra/khởi tạo bucket {Bucket} trên MinIO.", bucketName);
            throw;
        }
    }

    /// <summary>
    /// Cấu hình chính sách truy cập công khai (Public Read) cho bucket.
    /// Chỉ cấp quyền s3:GetObject và loại bỏ s3:ListBucket để tránh việc người dùng ẩn danh quét danh sách đối tượng.
    /// </summary>
    /// <param name="bucketName">Tên bucket cần cấu hình.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SetPublicReadPolicyAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = $$"""
            {
              "Version": "2012-10-17",
              "Statement": [
                {
                  "Effect": "Allow",
                  "Principal": "*",
                  "Action": [
                    "s3:GetObject"
                  ],
                  "Resource": [
                    "arn:aws:s3:::{{bucketName}}/*"
                  ]
                }
              ]
            }
            """;

            var spArgs = new SetPolicyArgs().WithBucket(bucketName).WithPolicy(policy);
            await _minioClient.SetPolicyAsync(spArgs, cancellationToken);
            _logger.LogInformation("Đã cấu hình chính sách Public Read cho S3 Bucket: {Bucket}", bucketName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể thiết lập bucket policy cho {Bucket}.", bucketName);
            throw;
        }
    }

    private static string GetDefaultContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".mp4" => "video/mp4",
        _ => "application/octet-stream"
    };
}
