using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Orders;
public static class OrderErrors
{
    public static readonly Error EmptyCart = Error.Validation("Orders.EmptyCart", "Chọn sản phẩm không rỗng, không trùng.");
    public static readonly Error ShippingProviderRequired = Error.Validation("Orders.ShippingProviderRequired", "Mỗi shop cần provider/serviceCode được adapter hỗ trợ.");
    public static readonly Error ZeroAmountNotAllowed = Error.Validation("Orders.ZeroAmountNotAllowed", "Tổng thanh toán phải lớn hơn 0 VND.");
    public static readonly Error CodeUnavailable = Error.Conflict("Orders.CodeUnavailable", "Không thể tạo mã đơn duy nhất. Vui lòng thử lại.");
    public static readonly Error CartChanged = Error.Conflict("Orders.CartChanged", "Giỏ hàng đã thay đổi. Vui lòng tính lại checkout.");
    public static readonly Error OrderNotFound = Error.NotFound("Orders.OrderNotFound", "Không tìm thấy đơn hàng của bạn.");
    public static readonly Error SubOrderNotFound = Error.NotFound("Orders.SubOrderNotFound", "Không tìm thấy đơn hàng của gian hàng.");
    public static readonly Error WarehouseNotFound = Error.NotFound("Orders.WarehouseNotFound", "Không tìm thấy kho thuộc gian hàng.");
    public static readonly Error ShopContextRequired = Error.Forbidden("Orders.ShopContextRequired", "Yêu cầu ngữ cảnh gian hàng.");
}
