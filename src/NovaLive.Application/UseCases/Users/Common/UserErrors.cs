using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Users.Common;

public static class UserErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "User.NotFound",
        "Không tìm thấy thông tin người dùng.");

    public static readonly Error Inactive = Error.Validation(
        "User.Inactive",
        "Tài khoản người dùng chưa được kích hoạt hoặc đã bị vô hiệu hóa.");

    public static readonly Error PhoneAlreadyInUse = Error.Conflict(
        "User.PhoneAlreadyInUse",
        "Số điện thoại này đã được sử dụng bởi một tài khoản khác.");

    public static readonly Error Unauthorized = Error.Unauthorized(
        "User.Unauthorized",
        "Bạn chưa đăng nhập hoặc không có quyền thực hiện thao tác này.");
}

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
