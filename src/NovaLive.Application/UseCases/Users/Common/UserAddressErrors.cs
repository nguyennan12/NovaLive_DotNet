using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Users.Common;

public static class UserAddressErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "UserAddress.NotFound",
        "Không tìm thấy địa chỉ nhận hàng.");

    public static readonly Error CannotDeleteDefaultAddress = Error.Validation(
        "UserAddress.CannotDeleteDefaultAddress",
        "Không thể xóa địa chỉ mặc định khi còn địa chỉ khác trong sổ địa chỉ");

    public static readonly Error Unauthorized = Error.Unauthorized(
        "UserAddress.Unauthorized",
        "Bạn không có quyền thao tác trên địa chỉ nhận hàng này.");
}
