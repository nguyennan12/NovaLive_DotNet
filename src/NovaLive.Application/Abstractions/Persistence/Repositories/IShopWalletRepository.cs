using NovaLive.Domain.Shops;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

/// <summary>
/// Repository quản lý truy vấn và cập nhật ví Shop (ShopWallet) và sổ cái giao dịch (ShopWalletTransaction).
/// </summary>
public interface IShopWalletRepository
{
    /// <summary>
    /// Tra cứu thông tin ví Shop theo mã định danh shopId.
    /// </summary>
    /// <param name="shopId">Mã định danh duy nhất của Shop.</param>
    /// <param name="ct">CancellationToken huỷ tác vụ.</param>
    /// <returns>Thực thể ShopWallet nếu tồn tại, ngược lại null.</returns>
    Task<ShopWallet?> GetByShopIdAsync(Guid shopId, CancellationToken ct = default);

    /// <summary>
    /// Tra cứu thông tin ví Shop theo mã định danh shopId đồng thời khoá dòng (SELECT FOR UPDATE) để đồng bộ giao dịch.
    /// </summary>
    /// <param name="shopId">Mã định danh duy nhất của Shop.</param>
    /// <param name="ct">CancellationToken huỷ tác vụ.</param>
    /// <returns>Thực thể ShopWallet nếu tồn tại, ngược lại null.</returns>
    Task<ShopWallet?> GetByShopIdForUpdateAsync(Guid shopId, CancellationToken ct = default);

    /// <summary>
    /// Thêm mới bản ghi ví Shop vào cơ sở dữ liệu.
    /// </summary>
    /// <param name="wallet">Thực thể ShopWallet cần lưu.</param>
    /// <param name="ct">CancellationToken huỷ tác vụ.</param>
    Task AddAsync(ShopWallet wallet, CancellationToken ct = default);

    /// <summary>
    /// Thêm mới bản ghi giao dịch biến động số dư ví vào sổ cái ShopWalletTransaction.
    /// </summary>
    /// <param name="transaction">Thực thể ShopWalletTransaction cần lưu.</param>
    /// <param name="ct">CancellationToken huỷ tác vụ.</param>
    Task AddTransactionAsync(ShopWalletTransaction transaction, CancellationToken ct = default);
}
