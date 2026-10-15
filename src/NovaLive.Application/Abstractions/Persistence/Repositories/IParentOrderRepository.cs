using NovaLive.Domain.Orders;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

/// <summary>
/// Repository quản lý truy vấn và cập nhật đơn hàng tổng (ParentOrder).
/// </summary>
public interface IParentOrderRepository
{
    /// <summary>
    /// Tìm ParentOrder theo Id và tùy chọn kiểm tra BuyerId.
    /// </summary>
    Task<ParentOrder?> FindAsync(Guid id, Guid? buyerId, CancellationToken ct);

    /// <summary>
    /// Tìm ParentOrder theo mã OrderCode và tùy chọn kiểm tra BuyerId.
    /// </summary>
    Task<ParentOrder?> FindByOrderCodeAsync(string orderCode, Guid? buyerId, CancellationToken ct);

    /// <summary>
    /// Lấy ParentOrder và khoá dòng để cập nhật (SELECT FOR UPDATE).
    /// </summary>
    Task<ParentOrder?> GetForUpdateAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Lấy ParentOrder theo OrderCode và khoá dòng để cập nhật (SELECT FOR UPDATE).
    /// </summary>
    Task<ParentOrder?> GetByOrderCodeForUpdateAsync(string orderCode, CancellationToken ct);

    /// <summary>
    /// Lấy danh sách phân trang các đơn hàng của một người mua.
    /// </summary>
    Task<(IReadOnlyList<ParentOrder> Items, long Total)> GetMinePagedAsync(Guid buyerId,
        NovaLive.Domain.Common.ParentOrderPaymentStatus? status, DateTimeOffset? from, DateTimeOffset? to, int page, int size, CancellationToken ct);

    /// <summary>
    /// Thêm đơn hàng tổng mới, xử lý xung đột khoá duy nhất mã đơn hàng.
    /// </summary>
    Task<bool> TryAddAsync(ParentOrder order, CancellationToken ct);
}
