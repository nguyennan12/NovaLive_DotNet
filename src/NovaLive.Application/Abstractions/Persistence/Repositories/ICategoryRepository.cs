using NovaLive.Domain.Products;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

/// <summary>
/// Repository quản lý truy vấn và thao tác dữ liệu với bảng Categories.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Lấy danh mục theo mã định danh (chỉ lấy danh mục đang hiển thị IsVisible = true).
    /// </summary>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh mục theo mã định danh với tùy chọn lấy cả danh mục bị ẩn.
    /// </summary>
    Task<Category?> GetByIdAsync(Guid id, bool includeInvisible, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách toàn bộ danh mục đang hiển thị, sắp xếp theo DisplayOrder tăng dần.
    /// </summary>
    Task<List<Category>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách toàn bộ danh mục với tùy chọn lấy cả danh mục bị ẩn.
    /// </summary>
    Task<List<Category>> GetAllAsync(bool includeInvisible, CancellationToken ct = default);

    /// <summary>
    /// Tìm danh mục theo đường dẫn thân thiện (slug).
    /// </summary>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra danh mục có tồn tại theo ID hay không.
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra danh mục có chứa danh mục con hay không.
    /// </summary>
    Task<bool> HasChildrenAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Thêm mới một bản ghi danh mục vào cơ sở dữ liệu.
    /// </summary>
    Task AddAsync(Category category, CancellationToken ct = default);

    /// <summary>
    /// Cập nhật thông tin danh mục.
    /// </summary>
    void Update(Category category);

    /// <summary>
    /// Xóa một danh mục khỏi cơ sở dữ liệu.
    /// </summary>
    void Delete(Category category);
}
