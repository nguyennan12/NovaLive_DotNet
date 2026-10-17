using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
namespace NovaLive.Application.UseCases.Returns;
public static class ReturnErrors
{
    public static readonly Error NotFound = Error.NotFound("Returns.NotFound", "Không tìm thấy yêu cầu trả hàng.");
    public static readonly Error SubOrderNotFound = Error.NotFound("Returns.SubOrderNotFound", "Không tìm thấy đơn hàng của bạn.");
    public static readonly Error Unauthorized = Error.Unauthorized("Returns.Unauthorized", "Vui lòng đăng nhập.");
    public static readonly Error ItemNotInSubOrder = Error.Validation("Returns.ItemNotInSubOrder", "Sản phẩm không thuộc đơn hàng cần trả.");
    public static readonly Error ActiveReturnExists = Error.Validation("Returns.ActiveReturnExists", "Đơn hàng đang có yêu cầu trả hàng được xử lý.");
    public static readonly Error QuantityExceeded = ReturnStateErrors.InvalidQuantity;
    public static readonly Error NotDelivered = ReturnStateErrors.NotDelivered;
    public static readonly Error Expired = ReturnStateErrors.Expired;
    public static readonly Error InvalidSnapshot = ReturnStateErrors.InvalidSnapshot;
    public static readonly Error InvalidStatus = ReturnStateErrors.InvalidStatus;
}
