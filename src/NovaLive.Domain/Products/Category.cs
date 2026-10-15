using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

/// <summary>
/// Thực thể biểu diễn danh mục sản phẩm đa cấp (Category hierarchy) trong sàn thương mại điện tử NovaLive.
/// </summary>
public sealed class Category : Entity
{
    private Category() { }

    /// <summary>
    /// Khởi tạo một đối tượng Category mới.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của danh mục.</param>
    /// <param name="name">Tên danh mục hiển thị.</param>
    /// <param name="slug">Đường dẫn thân thiện (slug) URL.</param>
    /// <param name="parentId">Mã định danh danh mục cha (null nếu là danh mục gốc).</param>
    /// <param name="iconUrl">Đường dẫn icon biểu tượng của danh mục.</param>
    /// <param name="displayOrder">Thứ tự hiển thị (ưu tiên số nhỏ trước).</param>
    /// <param name="isVisible">Trạng thái hiển thị công khai trên giao diện người dùng.</param>
    public Category(
        Guid id,
        string name,
        string slug,
        Guid? parentId = null,
        string? iconUrl = null,
        int displayOrder = 0,
        bool isVisible = true)
    {
        Id = id;
        Name = name;
        Slug = slug;
        ParentId = parentId;
        IconUrl = iconUrl;
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Mã định danh của danh mục cha. Giá trị null nếu danh mục này là danh mục gốc (root).
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// Tên hiển thị của danh mục.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Slug URL thân thiện của danh mục (duy nhất trên toàn hệ thống).
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Đường dẫn CDN chứa ảnh biểu tượng (icon) của danh mục.
    /// </summary>
    public string? IconUrl { get; private set; }

    /// <summary>
    /// Thứ tự sắp xếp hiển thị của danh mục trong cùng cấp phân cấp.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Cho biết danh mục có đang được hiển thị công khai tới người dùng hay không.
    /// </summary>
    public bool IsVisible { get; private set; } = true;

    /// <summary>
    /// Thời điểm khởi tạo bản ghi danh mục theo giờ chuẩn UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Cập nhật thông tin chi tiết của danh mục.
    /// </summary>
    /// <param name="name">Tên mới của danh mục.</param>
    /// <param name="slug">Slug mới của danh mục.</param>
    /// <param name="parentId">Mã danh mục cha mới.</param>
    /// <param name="iconUrl">Đường dẫn icon mới.</param>
    /// <param name="displayOrder">Thứ tự hiển thị mới.</param>
    /// <param name="isVisible">Trạng thái hiển thị mới.</param>
    public void Update(
        string name,
        string slug,
        Guid? parentId,
        string? iconUrl,
        int displayOrder,
        bool isVisible)
    {
        Name = name;
        Slug = slug;
        ParentId = parentId;
        IconUrl = iconUrl;
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
    }
}
