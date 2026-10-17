using NovaLive.Domain.Common;
namespace NovaLive.Domain.Orders;
public static class ReturnStateErrors
{
    public static readonly Error ResponseExpired = Error.Validation("Returns.ResponseExpired", "Đã quá thời hạn phản hồi trả hàng của Seller.");
    public static readonly Error RejectionRequired = Error.Validation("Returns.RejectionRequired", "Cần lý do và bằng chứng từ chối.");
    public static readonly Error InvalidStatus = Error.Validation("Returns.InvalidStatus", "Trạng thái yêu cầu trả hàng không cho phép thao tác này.");
    public static readonly Error NotDelivered = Error.Validation("Returns.NotDelivered", "Chỉ được trả hàng cho đơn đã giao.");
    public static readonly Error Expired = Error.Validation("Returns.Expired", "Đã hết thời hạn khiếu nại trả hàng");
    public static readonly Error InvalidQuantity = Error.Validation("Returns.QuantityExceeded", "Số lượng trả vượt quá số lượng đã mua.");
    public static readonly Error InvalidSnapshot = Error.Validation("Returns.InvalidSnapshot", "Snapshot giá không hợp lệ để tính tiền hoàn.");
}
