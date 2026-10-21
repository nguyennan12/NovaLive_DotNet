using NovaLive.Domain.Common;

namespace NovaLive.Domain.Reviews;

public static class ReviewErrors
{
    public static readonly Error NotFound = Error.NotFound("Reviews.NotFound", "Không tìm thấy đánh giá.");
    public static readonly Error OrderItemNotFound = Error.NotFound("Reviews.OrderItemNotFound", "Không tìm thấy sản phẩm đã mua.");
    public static readonly Error NotDelivered = Error.Validation("Reviews.NotDelivered", "Chỉ được đánh giá sản phẩm đã giao và chưa hoàn trả.");
    public static readonly Error AlreadyReviewed = Error.Conflict("Reviews.AlreadyReviewed", "Sản phẩm trong đơn hàng đã được đánh giá.");
    public static readonly Error AlreadyEdited = Error.Conflict("Reviews.AlreadyEdited", "Chỉ được sửa đánh giá một lần.");
    public static readonly Error EditExpired = Error.Validation("Reviews.EditExpired", "Đã quá 30 ngày để sửa đánh giá.");
    public static readonly Error Hidden = Error.Validation("Reviews.Hidden", "Không thể sửa đánh giá đang bị ẩn.");
    public static readonly Error AlreadyReplied = Error.Conflict("Reviews.AlreadyReplied", "Chỉ được phản hồi một lần.");
    public static readonly Error InvalidRating = Error.Validation("Reviews.InvalidRating", "Điểm đánh giá phải từ 1 đến 5.");
    public static readonly Error InvalidText = Error.Validation("Reviews.InvalidText", "Nội dung vượt quá giới hạn độ dài.");
    public static readonly Error InvalidMedia = Error.Validation("Reviews.InvalidMedia", "Ảnh/video phải là URL HTTPS hợp lệ với định dạng được hỗ trợ.");
    public static readonly Error InvalidReply = Error.Validation("Reviews.InvalidReply", "Phản hồi không được rỗng hoặc vượt quá giới hạn.");
    public static readonly Error InvalidReason = Error.Validation("Reviews.InvalidReason", "Lý do không được rỗng hoặc vượt quá giới hạn.");
    public static readonly Error InvalidIdentity = Error.Validation("Reviews.InvalidIdentity", "Mã đối tượng không hợp lệ.");
}
