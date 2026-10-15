namespace NovaLive.Application.Abstractions.Services;

/// <summary>
/// Giao diện dịch vụ lưu trữ tập tin media (ảnh, video) vào hệ thống Object Storage tương thích S3 (MinIO).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload file stream lên bucket chỉ định và trả về URL CDN đầy đủ.
    /// </summary>
    /// <param name="fileStream">Stream dữ liệu tập tin.</param>
    /// <param name="fileName">Tên tập tin gốc kèm phần mở rộng.</param>
    /// <param name="contentType">MIME type của tập tin.</param>
    /// <param name="bucketName">Tên bucket lưu trữ (mặc định: products).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Đường dẫn CDN URL đầy đủ để truy cập tập tin.</returns>
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string bucketName = StorageBuckets.Products,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload dữ liệu dạng stream với tên định danh objectName cụ thể.
    /// </summary>
    /// <param name="content">Stream nội dung tập tin.</param>
    /// <param name="objectName">Tên định danh của object trên bucket.</param>
    /// <param name="contentType">MIME type của tập tin.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Đường dẫn CDN URL đầy đủ.</returns>
    Task<string> UploadAsync(
        Stream content,
        string objectName,
        string contentType,
        CancellationToken cancellationToken);
}
