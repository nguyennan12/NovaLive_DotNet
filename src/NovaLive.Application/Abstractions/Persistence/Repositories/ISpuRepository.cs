using NovaLive.Domain.Products;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

/// <summary>
/// Repository quản lý truy vấn và thao tác dữ liệu với bảng SPUs (Sản phẩm tiêu chuẩn).
/// </summary>
public interface ISpuRepository
{
    /// <summary>
    /// Đếm số lượng SPU đang hoạt động của một cửa hàng.
    /// </summary>
    Task<int> CountActiveByShopAsync(Guid shopId, CancellationToken ct = default);

    /// <summary>
    /// Lấy SPU theo mã định danh duy nhất.
    /// </summary>
    Task<Spu?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách SPU theo tập hợp các mã định danh.
    /// </summary>
    Task<List<Spu>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    /// <summary>
    /// Lấy SPU thuộc về một shop cụ thể.
    /// </summary>
    Task<Spu?> GetByIdAndShopAsync(Guid id, Guid shopId, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách SPU phân trang theo các điều kiện lọc (shop, danh mục, từ khóa tìm kiếm).
    /// </summary>
    Task<(List<Spu> Items, long TotalCount)> GetPagedAsync(
        Guid shopId,
        Guid? categoryId,
        string? keyword,
        int page,
        int size,
        CancellationToken ct = default);

    /// <summary>
    /// Thêm mới một SPU vào cơ sở dữ liệu.
    /// </summary>
    Task AddAsync(Spu spu, CancellationToken ct = default);

    /// <summary>
    /// Cập nhật thông tin SPU.
    /// </summary>
    void Update(Spu spu);

    /// <summary>
    /// Xóa SPU khỏi cơ sở dữ liệu.
    /// </summary>
    void Delete(Spu spu);

    /// <summary>
    /// Kiểm tra xem có sản phẩm SPU nào đang thuộc về danh mục chỉ định hay không (dùng để chặn xóa category).
    /// </summary>
    Task<bool> ExistsByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>
    /// Đếm số lượng sản phẩm SPU cho từng danh mục theo danh sách category IDs (dùng để hiển thị ProductCount trên cây danh mục).
    /// </summary>
    Task<Dictionary<Guid, int>> GetProductCountsByCategoryIdsAsync(IEnumerable<Guid> categoryIds, CancellationToken ct = default);
}
