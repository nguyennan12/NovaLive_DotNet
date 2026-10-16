using NovaLive.Domain.Common;
namespace NovaLive.Domain.Orders;
public static class OrderStateErrors
{
    public static readonly Error AlreadyCancelled = Error.Conflict("Orders.AlreadyCancelled", "Đơn đã hủy.");
    public static readonly Error CannotCancelConfirmedOrder = Error.Conflict("Orders.CannotCancelConfirmedOrder", "Buyer chỉ được hủy khi mọi đơn con đang chờ thanh toán.");
    public static readonly Error InvalidStatusTransition = Error.Conflict("Orders.InvalidStatusTransition", "Trạng thái đơn không cho phép thao tác này.");
    public static readonly Error SubOrderNotPaid = Error.Conflict("Orders.SubOrderNotPaid", "Đơn chưa được thanh toán/xác nhận COD.");
    public static readonly Error CannotCancelPaidOrderYet = Error.Conflict("Orders.CannotCancelPaidOrderYet", "Đơn đã thanh toán cần luồng hoàn tiền T32/T38/T39.");
    public static readonly Error InvalidReason = Error.Validation("Orders.InvalidReason", "Lý do hủy bắt buộc, tối đa 500 ký tự.");
    public static readonly Error InvalidWarehouse = Error.Validation("Orders.InvalidWarehouse", "Kho không hợp lệ.");
}
