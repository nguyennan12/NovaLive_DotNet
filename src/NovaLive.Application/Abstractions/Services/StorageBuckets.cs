namespace NovaLive.Application.Abstractions.Services;

/// <summary>
/// Định nghĩa các tên bucket S3 được hỗ trợ trong hệ sinh thái NovaLive cùng các phân loại public/private.
/// </summary>
public static class StorageBuckets
{
    /// <summary>
    /// Bucket công khai chứa hình ảnh, video sản phẩm (SPU/SKU).
    /// </summary>
    public const string Products = "products";

    /// <summary>
    /// Bucket riêng tư (private) chứa hồ sơ KYC (CCCD, giấy phép kinh doanh) của người bán.
    /// </summary>
    public const string KycDocs = "kyc-docs";

    /// <summary>
    /// Bucket công khai chứa hình ảnh/video đánh giá từ người mua.
    /// </summary>
    public const string Reviews = "reviews";

    /// <summary>
    /// Bucket công khai chứa dữ liệu phát trực tiếp livestream (thumbnails, media).
    /// </summary>
    public const string Livestreams = "livestreams";

    /// <summary>
    /// Danh sách toàn bộ các bucket hợp lệ trong hệ thống.
    /// </summary>
    public static readonly string[] All = [Products, KycDocs, Reviews, Livestreams];

    /// <summary>
    /// Danh sách các bucket công khai (truy cập đọc qua CDN không cần xác thực).
    /// </summary>
    public static readonly string[] Public = [Products, Reviews, Livestreams];

    /// <summary>
    /// Danh sách các bucket riêng tư (yêu cầu phân quyền nghiêm ngặt).
    /// </summary>
    public static readonly string[] Private = [KycDocs];

    /// <summary>
    /// Kiểm tra tên bucket có nằm trong danh sách bucket hệ thống hỗ trợ hay không.
    /// </summary>
    public static bool IsValid(string bucketName) =>
        All.Contains(bucketName, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Kiểm tra tên bucket có phải là bucket công khai hay không.
    /// </summary>
    public static bool IsPublic(string bucketName) =>
        Public.Contains(bucketName, StringComparer.OrdinalIgnoreCase);
}
