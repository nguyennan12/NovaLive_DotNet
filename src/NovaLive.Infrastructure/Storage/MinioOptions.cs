namespace NovaLive.Infrastructure.Storage;

/// <summary>
/// Các thiết lập cấu hình kết nối tới kho lưu trữ đối tượng MinIO S3.
/// </summary>
public sealed class MinioOptions
{
    /// <summary>
    /// Tên mục cấu hình trong tệp cấu hình appsettings.json.
    /// </summary>
    public const string SectionName = "Minio";

    /// <summary>
    /// Địa chỉ endpoint máy chủ MinIO S3 (mặc định localhost:9000).
    /// </summary>
    public string Endpoint { get; set; } = "localhost:9000";

    /// <summary>
    /// Khóa truy cập (Access Key) xác thực S3. Được cung cấp qua biến môi trường hoặc user secrets.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Khóa bí mật (Secret Key) xác thực S3. Được cung cấp qua biến môi trường hoặc user secrets.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Cho biết có sử dụng giao thức bảo mật SSL/HTTPS để kết nối tới MinIO hay không.
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// URL cơ sở của mạng phân phối nội dung CDN dùng để tạo liên kết truy cập media công khai.
    /// </summary>
    public string CdnBaseUrl { get; set; } = "https://cdn.novalive.vn";
}
