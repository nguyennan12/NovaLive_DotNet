using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.FlashSales;

public static class FlashSalesErrors
{
    public static readonly Error CampaignNotFound =
        Error.NotFound("FlashSales.CampaignNotFound", "Không tìm thấy chiến dịch Flash Sale.");

    public static readonly Error CampaignNotScheduled =
        Error.Validation("FlashSales.CampaignNotScheduled", "Chiến dịch Flash Sale không ở trạng thái có thể đăng ký.");

    public static readonly Error CampaignRegistrationClosed =
        Error.Validation("FlashSales.CampaignRegistrationClosed", "Thời hạn đăng ký tham gia chiến dịch Flash Sale đã kết thúc.");

    public static readonly Error CampaignAlreadyEnded =
        Error.Validation("FlashSales.CampaignAlreadyEnded", "Chiến dịch Flash Sale đã kết thúc.");

    public static readonly Error CampaignCancelled =
        Error.Validation("FlashSales.CampaignCancelled", "Chiến dịch Flash Sale đã bị hủy.");

    public static readonly Error InvalidCampaignTimeRange =
        Error.Validation("FlashSales.InvalidCampaignTimeRange", "Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");

    public static readonly Error ItemNotFound =
        Error.NotFound("FlashSales.ItemNotFound", "Không tìm thấy sản phẩm đăng ký Flash Sale.");

    public static readonly Error ItemNotPending =
        Error.Validation("FlashSales.ItemNotPending", "Chỉ có thể phê duyệt hoặc từ chối sản phẩm đang ở trạng thái chờ duyệt (Pending).");

    public static readonly Error SkuNotFound =
        Error.NotFound("FlashSales.SkuNotFound", "Không tìm thấy thông tin SKU sản phẩm.");

    public static readonly Error SkuNotBelongToShop =
        Error.Forbidden("FlashSales.SkuNotBelongToShop", "SKU sản phẩm không thuộc quyền sở hữu của gian hàng.");

    public static readonly Error SkuNotActive =
        Error.Validation("FlashSales.SkuNotActive", "SKU sản phẩm hiện không ở trạng thái kích hoạt.");

    public static readonly Error FlashPriceMustBe10PercentLower =
        Error.Validation("FlashSales.FlashPriceMustBe10PercentLower", "Giá Flash Sale phải thấp hơn giá bán thường tối thiểu 10%.");

    public static readonly Error InsufficientInventoryStock =
        Error.Validation("FlashSales.InsufficientInventoryStock", "Số lượng tồn kho khả dụng của SKU không đủ để đăng ký tham gia Flash Sale.");

    public static readonly Error SkuOverlapsWithAnotherCampaign =
        Error.Conflict("FlashSales.SkuOverlapsWithAnotherCampaign", "SKU này đã được đăng ký hoặc phê duyệt trong một chiến dịch Flash Sale khác trùng khung giờ.");

    public static readonly Error SkuAlreadyRegisteredInCampaign =
        Error.Conflict("FlashSales.SkuAlreadyRegisteredInCampaign", "SKU này đã được đăng ký tham gia chiến dịch Flash Sale này.");

    public static readonly Error UnauthorizedShop =
        Error.Unauthorized("FlashSales.UnauthorizedShop", "Không tìm thấy thông tin gian hàng của người dùng.");

    public static readonly Error ForbiddenShop =
        Error.Forbidden("FlashSales.ForbiddenShop", "Bạn không có quyền thao tác trên gian hàng này.");

    public static readonly Error InvalidQuantity =
        Error.Validation("FlashSales.InvalidQuantity", "Số lượng đăng ký phải lớn hơn 0.");

    public static readonly Error InvalidPerUserLimit =
        Error.Validation("FlashSales.InvalidPerUserLimit", "Giới hạn mua mỗi người dùng phải lớn hơn 0.");

    public static readonly Error InvalidFlashPrice =
        Error.Validation("FlashSales.InvalidFlashPrice", "Giá Flash Sale phải lớn hơn 0.");
}
