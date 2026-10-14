using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops;

public static class ShopErrors
{
    public static readonly Error Unauthorized = Error.Unauthorized("Shops.Unauthorized", "Vui lòng đăng nhập.");
    public static readonly Error InactiveUser = Error.Forbidden("Shops.InactiveUser", "Tài khoản phải đang hoạt động.");
    public static readonly Error AlreadyOwned = Error.Validation("Shops.AlreadyOwned", "Mỗi tài khoản chỉ được sở hữu tối đa 1 gian hàng");
    public static readonly Error SlugTaken = Error.Conflict("Shops.SlugTaken", "Đường dẫn gian hàng đã được sử dụng.");
    public static readonly Error NotFound = Error.NotFound("Shops.NotFound", "Không tìm thấy gian hàng.");
    public static readonly Error CannotManage = Error.Forbidden("Shops.CannotManage", "Gian hàng hiện không cho phép chỉnh sửa.");
    public static readonly Error AddressNotFound = Error.NotFound("Shops.AddressNotFound", "Không tìm thấy kho hàng.");
    public static readonly Error LastPrimary = Error.Validation("Shops.LastPrimary", "Không thể xóa kho chính duy nhất.");
    public static readonly Error SelfFollow = Error.Validation("Shops.SelfFollow", "Không thể theo dõi gian hàng của chính mình.");
}
